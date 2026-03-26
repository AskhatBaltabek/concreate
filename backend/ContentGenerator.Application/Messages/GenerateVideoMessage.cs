namespace ContentGenerator.Application.Messages;

public record GenerateVideoMessage
{
    public Guid ProjectId { get; init; }
    public string ScriptText { get; init; } = string.Empty;
    public string AudioUrl { get; init; } = string.Empty;
}
