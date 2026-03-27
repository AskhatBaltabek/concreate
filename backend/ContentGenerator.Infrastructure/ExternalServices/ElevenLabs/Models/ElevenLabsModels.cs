using System.Text.Json.Serialization;

namespace ContentGenerator.Infrastructure.ExternalServices.ElevenLabs.Models;

public class ElevenLabsTextToSpeechRequest
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("model_id")]
    public string ModelId { get; set; } = string.Empty;

    [JsonPropertyName("voice_settings")]
    public VoiceSettings? VoiceSettings { get; set; }
}

public class VoiceSettings
{
    [JsonPropertyName("stability")]
    public double Stability { get; set; }

    [JsonPropertyName("similarity_boost")]
    public double SimilarityBoost { get; set; }
}
