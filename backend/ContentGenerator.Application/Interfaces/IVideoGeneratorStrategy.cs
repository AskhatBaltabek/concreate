namespace ContentGenerator.Application.Interfaces;

public interface IVideoGeneratorStrategy
{
    string ProviderName { get; }
    
    Task<string> GenerateVideoAsync(string prompt, string imageUrl, CancellationToken cancellationToken);
    
    // ExternalId is passed via generationId
    Task<string> CheckStatusAsync(string generationId, CancellationToken cancellationToken);
}
