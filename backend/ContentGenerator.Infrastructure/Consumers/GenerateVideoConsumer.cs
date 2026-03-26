using ContentGenerator.Application.Interfaces;
using ContentGenerator.Application.Messages;
using ContentGenerator.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContentGenerator.Infrastructure.Consumers;

public class GenerateVideoConsumer : IConsumer<GenerateVideoMessage>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVideoGeneratorStrategy _videoGenerator;
    private readonly IMediaStorageService _mediaStorage;

    public GenerateVideoConsumer(
        IServiceProvider serviceProvider,
        IVideoGeneratorStrategy videoGenerator,
        IMediaStorageService mediaStorage)
    {
        _serviceProvider = serviceProvider;
        _videoGenerator = videoGenerator;
        _mediaStorage = mediaStorage;
    }

    public async Task Consume(ConsumeContext<GenerateVideoMessage> context)
    {
        var msg = context.Message;

        var videoUrl = await _videoGenerator.GenerateVideoAsync(msg.ScriptText, msg.AudioUrl, context.CancellationToken);

        // Dummy downloaded video stream
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("[DUMMY FINAL GENERATED MP4 VIDEO FILE CONCATENATED]"));
        
        var fileName = $"video_{Guid.NewGuid()}.mp4";
        var savedPath = await _mediaStorage.SaveMediaAsync(msg.ProjectId, fileName, stream, context.CancellationToken);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == msg.ProjectId, context.CancellationToken);
        if (project != null)
        {
            var videoMedia = new MediaLayer
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Type = MediaType.Video,
                FilePath = savedPath,
                OrderSequence = 1,
                DurationMs = 15000
            };
            db.MediaLayers.Add(videoMedia);
            project.Status = ProjectStatus.Completed;
            await db.SaveChangesAsync(context.CancellationToken);
        }
    }
}
