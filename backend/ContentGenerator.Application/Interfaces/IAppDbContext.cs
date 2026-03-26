using ContentGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContentGenerator.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Project> Projects { get; }
    DbSet<VideoSettings> VideoSettings { get; }
    DbSet<ScriptLayer> Scripts { get; }
    DbSet<MediaLayer> MediaLayers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
