using System.Text;
using System.Text.Json;
using ContentGenerator.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContentGenerator.Infrastructure.Services;

public class GenAiProVoiceGeneratorService : IVoiceGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GenAiProVoiceGeneratorService> _logger;

    public string Provider => "genaipro";

    public GenAiProVoiceGeneratorService(HttpClient httpClient, IConfiguration configuration, ILogger<GenAiProVoiceGeneratorService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GenAiProSettings:ApiKey"] ?? "";
        _logger = logger;
    }

    public async Task<Stream> GenerateAudioAsync(string text, string voiceModelId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new Exception("GenAiPro API Key is missing in appsettings.json");
        }

        // 1. Create Task
        var createRequest = new
        {
            input = text,
            voice_id = string.IsNullOrEmpty(voiceModelId) ? "Kore" : voiceModelId, // Default voice if not provided
            model_id = "eleven_v3",
            speed = 1.0,
            similarity = 0.75,
            stability = 0.5
        };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        var createResponse = await _httpClient.PostAsync("https://genaipro.vn/api/v1/labs/task", 
            new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json"), cancellationToken);

        if (!createResponse.IsSuccessStatusCode)
        {
            var error = await createResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"GenAiPro Create Task Error: {error}");
        }

        var createJson = await createResponse.Content.ReadAsStringAsync(cancellationToken);
        using var createDoc = JsonDocument.Parse(createJson);
        var taskId = createDoc.RootElement.GetProperty("task_id").GetString() ?? "";

        if (string.IsNullOrEmpty(taskId)) throw new Exception("Failed to get task_id from GenAiPro");

        // 2. Poll for Result
        _logger.LogInformation("GenAiPro Task {TaskId} created, polling for completion...", taskId);
        
        string? audioUrl = null;
        int attempts = 0;
        const int maxAttempts = 90; // 90 attempts * 2 seconds = 180 seconds (3 minutes) max

        while (attempts < maxAttempts)
        {
            await Task.Delay(2000, cancellationToken);
            attempts++;

            var statusResponse = await _httpClient.GetAsync($"https://genaipro.vn/api/v1/labs/task/{taskId}", cancellationToken);
            if (!statusResponse.IsSuccessStatusCode) 
            {
                _logger.LogWarning("GenAiPro Polling - Failed to get status for {TaskId}: {StatusCode}", taskId, statusResponse.StatusCode);
                continue;
            }

            var statusJson = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
            using var statusDoc = JsonDocument.Parse(statusJson);
            var status = statusDoc.RootElement.GetProperty("status").GetString();

            _logger.LogInformation("GenAiPro Task {TaskId} status (Attempt {Attempt}/{Max}): {Status}", taskId, attempts, maxAttempts, status);

            if (status == "completed")
            {
                audioUrl = statusDoc.RootElement.GetProperty("result").GetString() ?? "";
                _logger.LogInformation("GenAiPro Task {TaskId} completed!", taskId);
                break;
            }
            else if (status == "failed")
            {
                _logger.LogError("GenAiPro Task {TaskId} failed on server side.", taskId);
                throw new Exception("GenAiPro audio generation task failed.");
            }
        }

        if (string.IsNullOrEmpty(audioUrl))
        {
            _logger.LogError("GenAiPro Task {TaskId} timed out after {Attempts} attempts.", taskId, attempts);
            throw new Exception($"GenAiPro audio generation timed out after {attempts * 2} seconds.");
        }

        // 3. Download Audio
        var audioBytes = await _httpClient.GetByteArrayAsync(audioUrl, cancellationToken);
        return new MemoryStream(audioBytes);
    }
}
