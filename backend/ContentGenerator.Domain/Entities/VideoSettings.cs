namespace ContentGenerator.Domain.Entities;

public class VideoSettings
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int LengthSeconds { get; set; } = 30; // usually 30, 60, 90
    public string Format { get; set; } = "9:16"; // Vertical by default for shorts
    public string Genre { get; set; } = "Educational";
    public string TargetAudience { get; set; } = "General";
    
    public string VoiceModelId { get; set; } = string.Empty; // e.g., ElevenLabs voice id
    public string VideoModel { get; set; } = "Luma"; // Strategy identifier
}
