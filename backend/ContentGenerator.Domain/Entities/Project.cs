namespace ContentGenerator.Domain.Entities;

public enum ProjectStatus
{
    Draft,
    GeneratingScript,
    UserReview,
    GeneratingAudio,
    GeneratingVideo,
    Completed,
    Failed
}

public class Project
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    
    // One-to-One
    public VideoSettings Settings { get; set; } = null!;
    public ScriptLayer? Script { get; set; }
    
    // One-to-Many
    public ICollection<MediaLayer> MediaLayers { get; set; } = new List<MediaLayer>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
