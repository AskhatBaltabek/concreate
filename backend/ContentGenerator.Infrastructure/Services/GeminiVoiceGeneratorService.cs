using System.Text;
using System.Text.Json;
using ContentGenerator.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ContentGenerator.Infrastructure.Services;

public class GeminiVoiceGeneratorService : IVoiceGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public string Provider => "gemini";

    public GeminiVoiceGeneratorService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? "";
        _model = configuration["GeminiSettings:VoiceModel"] ?? "gemini-2.5-flash-preview-tts";
    }

    public async Task<Stream> GenerateAudioAsync(string text, string voiceModelId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey.Contains("YOUR_GEMINI_API_KEY"))
        {
            throw new Exception("Please provide a valid Gemini API key in appsettings.json.");
        }

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text } } }
            },
            generationConfig = new
            {
                responseModalities = new[] { "AUDIO" },
                speechConfig = new
                {
                    voiceConfig = new
                    {
                        prebuiltVoiceConfig = new
                        {
                            voiceName = string.IsNullOrEmpty(voiceModelId) ? "Puck" : voiceModelId
                        }
                    }
                }
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
        var response = await _httpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Gemini Audio API Error ({(int)response.StatusCode}): {errorBody}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(jsonResponse);
        
        try 
        {
            var firstPart = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0];

            if (firstPart.TryGetProperty("inlineData", out var inlineData))
            {
                var mimeType = inlineData.GetProperty("mimeType").GetString();
                var base64Audio = inlineData.GetProperty("data").GetString();
                if (string.IsNullOrEmpty(base64Audio))
                {
                    throw new Exception("Empty audio data returned from Gemini.");
                }
                var audioBytes = Convert.FromBase64String(base64Audio);
                return new MemoryStream(audioBytes);
            }
            else if (firstPart.TryGetProperty("text", out var textResponse))
            {
                var responseText = textResponse.GetString() ?? "";
                var preview = responseText.Length > 100 ? responseText[..100] + "..." : responseText;
                throw new Exception($"Gemini returned text instead of audio. Speech might not be supported for this model or text. Response: {preview}");
            }
            else
            {
                throw new Exception("Gemini response did not contain audio or text data.");
            }
        }
        catch (Exception ex) when (ex.GetType().Name == "JsonException")
        {
            throw new Exception($"Error parsing Gemini Audio response: {ex.Message}. Raw: {jsonResponse}");
        }
    }
}
