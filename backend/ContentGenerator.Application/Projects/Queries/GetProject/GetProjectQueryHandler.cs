using ContentGenerator.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentGenerator.Application.Projects.Queries.GetProject;

public class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, ProjectDto>
{
    private readonly IAppDbContext _context;

    public GetProjectQueryHandler(IAppDbContext context) => _context = context;

    public async Task<ProjectDto> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Script)
            .Include(p => p.MediaLayers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
            
        if (project == null) throw new Exception("Project not found");
        
        var audioUrl = project.MediaLayers.FirstOrDefault(m => m.Type == Domain.Entities.MediaType.Audio)?.FilePath;
        if (audioUrl != null) audioUrl = $"/media/{audioUrl}";
        
        var videoUrl = project.MediaLayers.FirstOrDefault(m => m.Type == Domain.Entities.MediaType.Video)?.FilePath;
        if (videoUrl != null) videoUrl = $"/media/{videoUrl}";
        
        return new ProjectDto(project.Id, project.Title, project.Description, (int)project.Status, project.Script?.GeneratedScript, audioUrl, videoUrl);
    }
}
