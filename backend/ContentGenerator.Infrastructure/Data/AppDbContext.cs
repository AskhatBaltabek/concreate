using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ContentGenerator.Domain.Entities;
using ContentGenerator.Application.Interfaces;

namespace ContentGenerator.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IAppDbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<VideoSettings> VideoSettings { get; set; }
    public DbSet<ScriptLayer> Scripts { get; set; }
    public DbSet<MediaLayer> MediaLayers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Project>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);

        builder.Entity<Project>()
            .HasOne(p => p.Settings)
            .WithOne(s => s.Project)
            .HasForeignKey<VideoSettings>(s => s.ProjectId);

        builder.Entity<Project>()
            .HasOne(p => p.Script)
            .WithOne(s => s.Project)
            .HasForeignKey<ScriptLayer>(s => s.ProjectId);

        builder.Entity<Project>()
            .HasMany(p => p.MediaLayers)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId);
    }
}
