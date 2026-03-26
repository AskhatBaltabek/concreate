namespace ContentGenerator.Domain.Entities;

public class ScriptLayer
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public string RawPrompt { get; set; } = string.Empty; 
    public string GeneratedScript { get; set; } = string.Empty; 
    public string VoiceOverText { get; set; } = string.Empty; 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
