namespace ContentGenerator.Application.Messages;

public record GenerateAudioMessage
{
    public Guid ProjectId { get; init; }
    public string ScriptText { get; init; } = string.Empty;
    public string? VoiceModelId { get; init; }
    public string Provider { get; init; } = "genaipro";
}
