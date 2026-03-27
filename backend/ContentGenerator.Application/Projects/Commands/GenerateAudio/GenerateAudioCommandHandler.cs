using ContentGenerator.Application.Interfaces;
using ContentGenerator.Application.Messages;
using ContentGenerator.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContentGenerator.Application.Projects.Commands.GenerateAudio;

public class GenerateAudioCommandHandler : IRequestHandler<GenerateAudioCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<GenerateAudioCommandHandler> _logger;

    public GenerateAudioCommandHandler(
        IAppDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<GenerateAudioCommandHandler> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<string> Handle(GenerateAudioCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GenerateAudioCommand received for project {ProjectId} with provider {Provider}", request.ProjectId, request.Provider);

        var project = await _context.Projects
            .Include(p => p.Script)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null) 
        {
            _logger.LogError("Project {ProjectId} not found", request.ProjectId);
            throw new Exception("Project not found");
        }

        if (project.Script != null)
        {
            project.Script.GeneratedScript = request.ScriptText;
            project.Script.VoiceOverText = ContentGenerator.Application.Utils.ScriptParser.ExtractVoiceOver(request.ScriptText);
            project.Script.UpdatedAt = DateTime.UtcNow;
        }

        project.Status = ProjectStatus.GeneratingAudio;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Publishing GenerateAudioMessage for project {ProjectId}", project.Id);

        await _publishEndpoint.Publish(new GenerateAudioMessage
        {
            ProjectId = project.Id,
            ScriptText = request.ScriptText,
            VoiceModelId = request.VoiceModelId,
            Provider = request.Provider ?? "genaipro"
        }, cancellationToken);

        _logger.LogInformation("GenerateAudioMessage published successfully for project {ProjectId}", project.Id);

        return "queued";
    }
}
