using ContentGenerator.Application.Interfaces;
using ContentGenerator.Application.Messages;
using ContentGenerator.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ContentGenerator.Infrastructure.Consumers;

public class GenerateAudioConsumer : IConsumer<GenerateAudioMessage>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IVoiceGeneratorService> _voiceGenerators;
    private readonly IMediaStorageService _mediaStorage;
    private readonly ILogger<GenerateAudioConsumer> _logger;

    public GenerateAudioConsumer(
        IServiceProvider serviceProvider,
        IEnumerable<IVoiceGeneratorService> voiceGenerators,
        IMediaStorageService mediaStorage,
        ILogger<GenerateAudioConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _voiceGenerators = voiceGenerators;
        _mediaStorage = mediaStorage;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GenerateAudioMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing GenerateAudioMessage for project {ProjectId} with provider {Provider}", msg.ProjectId, msg.Provider);

        try
        {
            var provider = msg.Provider.ToLower();
            _logger.LogInformation("Available voice generators: {Providers}", string.Join(", ", _voiceGenerators.Select(g => g.Provider)));
            
            var voiceGenerator = _voiceGenerators.FirstOrDefault(g => g.Provider == provider)
                                ?? _voiceGenerators.FirstOrDefault(g => g.Provider == "genaipro")
                                ?? _voiceGenerators.First();

            _logger.LogInformation("Selected voice generator: {SelectedProvider}", voiceGenerator.Provider);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var project = await db.Projects
                .Include(p => p.Script)
                .FirstOrDefaultAsync(p => p.Id == msg.ProjectId, context.CancellationToken);

            if (project == null)
            {
                _logger.LogError("Project {ProjectId} not found", msg.ProjectId);
                return;
            }

            var textToSynthesize = !string.IsNullOrEmpty(project.Script?.VoiceOverText) 
                ? project.Script.VoiceOverText 
                : msg.ScriptText;

            if (string.IsNullOrEmpty(textToSynthesize))
            {
                _logger.LogError("No text to synthesize for project {ProjectId}", msg.ProjectId);
                throw new Exception("No text to synthesize");
            }

            _logger.LogInformation("Generating audio for project {ProjectId}, text length: {TextLength}", msg.ProjectId, textToSynthesize.Length);
            
            var audioStream = await voiceGenerator.GenerateAudioAsync(textToSynthesize, msg.VoiceModelId ?? "", context.CancellationToken);

            _logger.LogInformation("Audio generated successfully for project {ProjectId}, stream length: {StreamLength}", msg.ProjectId, audioStream.Length);

            var fileName = $"voiceover_{Guid.NewGuid()}.mp3";
            var savedPath = await _mediaStorage.SaveMediaAsync(project.Id, fileName, audioStream, context.CancellationToken);

            _logger.LogInformation("Audio saved to path: {SavedPath}", savedPath);

            var voiceMedia = new MediaLayer
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Type = MediaType.Audio,
                FilePath = savedPath,
                OrderSequence = 1,
                DurationMs = 5000
            };

            db.MediaLayers.Add(voiceMedia);
            project.Status = ProjectStatus.GeneratingVideo;
            
            await db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Audio generated and saved for project {ProjectId}", msg.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audio for project {ProjectId}", msg.ProjectId);
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == msg.ProjectId);
                if (project != null)
                {
                    project.Status = ProjectStatus.Failed;
                    await db.SaveChangesAsync(context.CancellationToken);
                }
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Error updating project status to Failed for project {ProjectId}", msg.ProjectId);
            }
        }
    }
}
