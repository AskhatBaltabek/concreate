using ContentGenerator.Application.Interfaces;
using System.Text;

namespace ContentGenerator.Infrastructure.Services.Mocks;

public class MockVoiceGeneratorService : IVoiceGeneratorService
{
    public string Provider => "mock";

    public async Task<Stream> GenerateAudioAsync(string text, string voiceModelId, CancellationToken cancellationToken)
    {
        await Task.Delay(1500, cancellationToken);
        
        var bytes = Encoding.UTF8.GetBytes($"[DUMMY AUDIO CONTENT USING VOICE MODEL {voiceModelId} FOR TEXT: {text}]");
        return new MemoryStream(bytes);
    }
}
