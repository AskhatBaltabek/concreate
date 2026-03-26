namespace ContentGenerator.Application.Interfaces;

public interface IVoiceGeneratorService
{
    string Provider { get; }
    Task<Stream> GenerateAudioAsync(string text, string voiceModelId, CancellationToken cancellationToken);
}
