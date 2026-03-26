using ContentGenerator.Application.Interfaces;

namespace ContentGenerator.Infrastructure.Services.Mocks;

public class MockLumaVideoGeneratorService : IVideoGeneratorStrategy
{
    public string ProviderName => "LumaMock";

    public async Task<string> GenerateVideoAsync(string prompt, string imageUrl, CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken); // Simulating long generation
        return "mock_video_url_from_luma";
    }

    public Task<string> CheckStatusAsync(string generationId, CancellationToken cancellationToken)
    {
        return Task.FromResult("Completed");
    }
}
