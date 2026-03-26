namespace ContentGenerator.Domain.Entities;

public enum MediaType
{
    Audio,
    Video,
    Image,
    BackgroundMusic
}

public class MediaLayer
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public MediaType Type { get; set; }
    public string FilePath { get; set; } = string.Empty; 
    public int OrderSequence { get; set; } 
    public int DurationMs { get; set; }
    
    public string ExternalId { get; set; } = string.Empty;
}
