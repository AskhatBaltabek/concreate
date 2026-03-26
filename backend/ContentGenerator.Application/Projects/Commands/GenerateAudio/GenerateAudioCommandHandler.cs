using ContentGenerator.Application.Interfaces;
using ContentGenerator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentGenerator.Application.Projects.Commands.GenerateAudio;

public class GenerateAudioCommandHandler : IRequestHandler<GenerateAudioCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly IEnumerable<IVoiceGeneratorService> _voiceGenerators;
    private readonly IMediaStorageService _mediaStorage;

    public GenerateAudioCommandHandler(
        IAppDbContext context,
        IEnumerable<IVoiceGeneratorService> voiceGenerators,
        IMediaStorageService mediaStorage)
    {
        _context = context;
        _voiceGenerators = voiceGenerators;
        _mediaStorage = mediaStorage;
    }

    public async Task<string> Handle(GenerateAudioCommand request, CancellationToken cancellationToken)
    {
        var provider = request.Provider?.ToLower() ?? "genaipro";
        var voiceGenerator = _voiceGenerators.FirstOrDefault(g => g.Provider == provider)
                            ?? _voiceGenerators.FirstOrDefault(g => g.Provider == "genaipro")
                            ?? _voiceGenerators.First();

        var project = await _context.Projects
            .Include(p => p.Settings)
            .Include(p => p.Script)
            .Include(p => p.MediaLayers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null) throw new Exception("Project not found");

        if (project.Script != null)
        {
            project.Script.GeneratedScript = request.ScriptText;
            project.Script.VoiceOverText = ContentGenerator.Application.Utils.ScriptParser.ExtractVoiceOver(request.ScriptText);
            project.Script.UpdatedAt = DateTime.UtcNow;
        }

        project.Status = ProjectStatus.GeneratingAudio;
        await _context.SaveChangesAsync(cancellationToken);

        // Use VoiceOverText if it's not empty, otherwise fallback to full text (if parsing failed or script is plain)
        var textToSynthesize = !string.IsNullOrEmpty(project.Script?.VoiceOverText) 
            ? project.Script.VoiceOverText 
            : request.ScriptText;

        var audioStream = await voiceGenerator.GenerateAudioAsync(textToSynthesize, project.Settings.VoiceModelId, cancellationToken);

        var fileName = $"voiceover_{Guid.NewGuid()}.mp3";
        var savedPath = await _mediaStorage.SaveMediaAsync(project.Id, fileName, audioStream, cancellationToken);

        var voiceMedia = new MediaLayer
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Type = MediaType.Audio,
            FilePath = savedPath,
            OrderSequence = 1,
            DurationMs = 5000 
        };

        _context.MediaLayers.Add(voiceMedia);
        project.Status = ProjectStatus.GeneratingVideo; // Advance state

        await _context.SaveChangesAsync(cancellationToken);

        return _mediaStorage.GetMediaUrl(savedPath);
    }
}
