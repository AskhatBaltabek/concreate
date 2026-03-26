using ContentGenerator.Application.Interfaces;
using ContentGenerator.Application.Projects.Queries.GetProject;
using ContentGenerator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentGenerator.Application.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IAppDbContext _context;

    public GetProjectsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return new List<ProjectDto>();
            
        var projects = await _context.Projects
            .Include(p => p.Script)
            .Include(p => p.MediaLayers)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return projects.Select(project => {
            var audioUrl = project.MediaLayers.FirstOrDefault(m => m.Type == MediaType.Audio)?.FilePath;
            if (audioUrl != null) audioUrl = $"/media/{audioUrl}";
            
            var videoUrl = project.MediaLayers.FirstOrDefault(m => m.Type == MediaType.Video)?.FilePath;
            if (videoUrl != null) videoUrl = $"/media/{videoUrl}";
            
            return new ProjectDto(
                project.Id, 
                project.Title, 
                project.Description, 
                (int)project.Status, 
                project.Script?.GeneratedScript, 
                audioUrl, 
                videoUrl);
        }).ToList();
    }
}
