using System.Text.Json.Serialization;

namespace ContentGenerator.Infrastructure.ExternalServices.GenAiPro.Models;

public class GenAiProCreateTaskRequest
{
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; } = string.Empty;

    [JsonPropertyName("model_id")]
    public string ModelId { get; set; } = string.Empty;

    [JsonPropertyName("speed")]
    public double Speed { get; set; }

    [JsonPropertyName("similarity")]
    public double Similarity { get; set; }

    [JsonPropertyName("stability")]
    public double Stability { get; set; }
}

public class GenAiProTaskResponse
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = string.Empty;
}

public class GenAiProStatusResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public string? Result { get; set; }
}
