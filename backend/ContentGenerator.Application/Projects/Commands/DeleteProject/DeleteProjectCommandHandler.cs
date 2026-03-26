using ContentGenerator.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentGenerator.Application.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly IMediaStorageService _mediaStorage;

    public DeleteProjectCommandHandler(IAppDbContext context, IMediaStorageService mediaStorage)
    {
        _context = context;
        _mediaStorage = mediaStorage;
    }

    public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.MediaLayers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.UserId == request.UserId, cancellationToken);

        if (project == null) return false;

        // Delete files from storage
        foreach (var layer in project.MediaLayers)
        {
            await _mediaStorage.DeleteMediaAsync(layer.FilePath, cancellationToken);
        }

        // Delete files in project directory if any remains or the directory itself
        // (The DeleteMediaAsync handles individual files, but we might want to ensure the folder is gone if the service supports it)
        // For now, individual files are deleted.

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
