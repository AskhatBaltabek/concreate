namespace ContentGenerator.Infrastructure.ExternalServices.ElevenLabs.Models;

public class ElevenLabsSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.elevenlabs.io/v1";
    public string DefaultVoiceId { get; set; } = "JBFqnCBsd6RMkjVDRZzb"; // Rachel
    public string DefaultModelId { get; set; } = "eleven_multilingual_v2";
    public double Stability { get; set; } = 0.5;
    public double SimilarityBoost { get; set; } = 0.75;
}
