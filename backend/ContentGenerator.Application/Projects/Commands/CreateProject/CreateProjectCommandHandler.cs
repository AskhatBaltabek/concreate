using ContentGenerator.Application.Interfaces;
using ContentGenerator.Domain.Entities;
using MediatR;

namespace ContentGenerator.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateProjectCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            UserId = request.UserId,
            Status = ProjectStatus.Draft,
            Settings = new VideoSettings
            {
                Id = Guid.NewGuid(),
                LengthSeconds = request.LengthSeconds,
                Format = request.Format
            }
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
