using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using ContentGenerator.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ContentGenerator.Infrastructure.Services;

public class ClaudeScriptGeneratorService : IScriptGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public string Provider => "claude";

    public ClaudeScriptGeneratorService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ClaudeSettings:ApiKey"] ?? "";
        _model = configuration["ClaudeSettings:Model"] ?? "claude-3-5-sonnet-20240620";
    }

    public async Task<string> GenerateScriptAsync(string topic, string targetAudience, int lengthSeconds, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_CLAUDE_API_KEY")
        {
            // Fallback for demo if key not provided
            return "Please provide a valid Claude API key in appsettings.json to generate real scripts.";
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

        var userPrompt = $@"Topic: {topic}
Target Audience: {targetAudience}
Duration: approximately {lengthSeconds} seconds.";

        var requestBody = new
        {
            model = _model,
            max_tokens = 2048,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (errorBody.Contains("credit balance is too low"))
            {
                throw new Exception("Claude API: Недостаточно средств на балансе. Пожалуйста, пополните баланс в Anthropic Console (Plans & Billing).");
            }
            throw new Exception($"Claude API Error ({(int)response.StatusCode} {response.StatusCode}): {errorBody}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(jsonResponse);
        
        // Extract content from Claude's response structure
        // Content is usually an array of items, first item has 'text'
        var content = document.RootElement.GetProperty("content")[0].GetProperty("text").GetString();

        return content ?? "Failed to generate script.";
    }
}
