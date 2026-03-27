using System.Net.Http.Json;
using ContentGenerator.Application.Interfaces;
using ContentGenerator.Infrastructure.ExternalServices.ElevenLabs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContentGenerator.Infrastructure.Services;

public class ElevenLabsVoiceGeneratorService : IVoiceGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ElevenLabsVoiceGeneratorService> _logger;
    private readonly ElevenLabsSettings _settings;

    public string Provider => "elevenlabs";

    public ElevenLabsVoiceGeneratorService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ElevenLabsVoiceGeneratorService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = new ElevenLabsSettings
        {
            ApiKey = configuration["ElevenLabsSettings:ApiKey"] ?? "",
            BaseUrl = configuration["ElevenLabsSettings:BaseUrl"] ?? "https://api.elevenlabs.io/v1",
            DefaultModelId = configuration["ElevenLabsSettings:DefaultModelId"] ?? "eleven_multilingual_v2",
        };
    }

    public async Task<Stream> GenerateAudioAsync(string text, string voiceModelId, CancellationToken cancellationToken)
    {
        ValidateSettings();

        var voiceId = string.IsNullOrWhiteSpace(voiceModelId) ? _settings.DefaultVoiceId : voiceModelId;
        var request = new ElevenLabsTextToSpeechRequest
        {
            Text = text,
            ModelId = _settings.DefaultModelId,
        };

        var url = $"{_settings.BaseUrl}/text-to-speech/{voiceId}";
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("xi-api-key", _settings.ApiKey);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("ElevenLabs TTS Error: {StatusCode} - {Error}", response.StatusCode, error);
            throw new Exception($"ElevenLabs TTS Error: {response.StatusCode} - {error}");
        }

        var audioBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return new MemoryStream(audioBytes);
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("ElevenLabs API Key is missing in configuration.");
        }
    }
}
