namespace ContentGenerator.Application.Interfaces;

public interface IScriptGeneratorService
{
    string Provider { get; }
    Task<string> GenerateScriptAsync(string topic, string targetAudience, int lengthSeconds, CancellationToken cancellationToken);
}
