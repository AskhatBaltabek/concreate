using ContentGenerator.Application.Interfaces;
using ContentGenerator.Application.Messages;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentGenerator.Application.Projects.Commands.GenerateVideo;

public class GenerateVideoCommandHandler : IRequestHandler<GenerateVideoCommand>
{
    private readonly IAppDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public GenerateVideoCommandHandler(IAppDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(GenerateVideoCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Script)
            .Include(p => p.MediaLayers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null) throw new Exception("Project not found");

        var audioLayer = project.MediaLayers.FirstOrDefault(m => m.Type == Domain.Entities.MediaType.Audio);
        if (audioLayer == null) throw new Exception("Audio not generated yet");

        project.Status = Domain.Entities.ProjectStatus.GeneratingVideo;
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new GenerateVideoMessage
        {
            ProjectId = project.Id,
            ScriptText = project.Script?.GeneratedScript ?? "",
            AudioUrl = audioLayer.FilePath
        }, cancellationToken);
    }
}
