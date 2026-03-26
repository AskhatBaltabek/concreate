using System.Text;
using System.Text.Json;
using ContentGenerator.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ContentGenerator.Infrastructure.Services;

public class GeminiScriptGeneratorService : IScriptGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public string Provider => "gemini";

    public GeminiScriptGeneratorService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? "";
        _model = configuration["GeminiSettings:Model"] ?? "gemini-1.5-flash";
    }

    public async Task<string> GenerateScriptAsync(string topic, string targetAudience, int lengthSeconds, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY")
        {
            return "Please provide a valid Gemini API key in appsettings.json.";
        }

        var systemPrompt = @"You are a professional video scriptwriter. 
Generate a structured video script. Use the following strict format for each scene:
[SCENE X]
Visual: [Describe the visual scene here]
Voiceover: ""[Write the exact text to be spoken here]""

Important: 
1. The text in the Voiceover section must be enclosed in double quotes.
2. Only the text inside the Voiceover section will be used for audio generation.
3. Make it engaging and optimized for the target audience.";

        var fullPrompt = $@"{systemPrompt}

Topic: {topic}
Target Audience: {targetAudience}
Duration: approximately {lengthSeconds} seconds.";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = fullPrompt } } }
            },
            generationConfig = new
            {
                maxOutputTokens = 2048,
                temperature = 0.7
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
        var response = await _httpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Gemini API Error ({(int)response.StatusCode}): {errorBody}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(jsonResponse);
        
        // Extract content from Gemini's response structure
        // candidates[0].content.parts[0].text
        try 
        {
            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            return text ?? "Failed to generate script.";
        }
        catch (Exception ex)
        {
            throw new Exception($"Error parsing Gemini response: {ex.Message}. Raw: {jsonResponse}");
        }
    }
}
