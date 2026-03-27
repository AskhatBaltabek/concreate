using System.Net.Http.Json;
using ContentGenerator.Application.Interfaces;
using ContentGenerator.Infrastructure.ExternalServices.GenAiPro.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContentGenerator.Infrastructure.Services;

public class GenAiProVoiceGeneratorService : IVoiceGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GenAiProVoiceGeneratorService> _logger;
    private readonly GenAiProSettings _settings;

    public string Provider => "genaipro";

    public GenAiProVoiceGeneratorService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GenAiProVoiceGeneratorService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = new GenAiProSettings
        {
            ApiKey = configuration["GenAiProSettings:ApiKey"] ?? "",
            BaseUrl = configuration["GenAiProSettings:BaseUrl"] ?? "https://genaipro.vn/api/v1/labs",
            DefaultVoiceId = configuration["GenAiProSettings:DefaultVoiceId"] ?? "airYK6ydeWdrJg6gyZA3",
            DefaultModelId = configuration["GenAiProSettings:DefaultModelId"] ?? "eleven_v3",
            DefaultSpeed = double.TryParse(configuration["GenAiProSettings:DefaultSpeed"], out var speed) ? speed : 1.2,
            DefaultSimilarity = double.TryParse(configuration["GenAiProSettings:DefaultSimilarity"], out var similarity) ? similarity : 0.75,
            DefaultStability = double.TryParse(configuration["GenAiProSettings:DefaultStability"], out var stability) ? stability : 0.5,
            MaxPollingAttempts = int.TryParse(configuration["GenAiProSettings:MaxPollingAttempts"], out var maxAttempts) ? maxAttempts : 180,
            PollingIntervalMs = int.TryParse(configuration["GenAiProSettings:PollingIntervalMs"], out var interval) ? interval : 2000
        };
    }

    public async Task<Stream> GenerateAudioAsync(string text, string voiceModelId, CancellationToken cancellationToken)
    {
        ValidateSettings();

        var taskId = await CreateTaskAsync(text, voiceModelId, cancellationToken);
        return await PollForCompletionAndDownloadAsync(taskId, cancellationToken);
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("GenAiPro API Key is missing in configuration.");
        }
    }

    private async Task<string> CreateTaskAsync(string text, string voiceModelId, CancellationToken cancellationToken)
    {
        var request = new GenAiProCreateTaskRequest
        {
            Input = text,
            VoiceId = string.IsNullOrWhiteSpace(voiceModelId) ? _settings.DefaultVoiceId : voiceModelId,
            ModelId = _settings.DefaultModelId,
            Speed = _settings.DefaultSpeed,
            Similarity = _settings.DefaultSimilarity,
            Stability = _settings.DefaultStability
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/task")
        {
            Content = JsonContent.Create(request)
        };
        AddAuthHeaders(httpRequest);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("GenAiPro Create Task Error: {Error}", error);
            throw new Exception($"GenAiPro Create Task Error: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<GenAiProTaskResponse>(cancellationToken: cancellationToken);
        if (string.IsNullOrEmpty(result?.TaskId))
        {
            throw new Exception("Failed to get task_id from GenAiPro response.");
        }

        return result.TaskId;
    }

    private async Task<Stream> PollForCompletionAndDownloadAsync(string taskId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GenAiPro Task {TaskId} created, polling for completion...", taskId);

        for (var attempt = 1; attempt <= _settings.MaxPollingAttempts; attempt++)
        {
            await Task.Delay(_settings.PollingIntervalMs, cancellationToken);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_settings.BaseUrl}/task/{taskId}");
            AddAuthHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            _logger.LogWarning("{response}", response);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GenAiPro Polling - Failed to get status for {TaskId}: {StatusCode}", taskId, response.StatusCode);
                continue;
            }

            var statusResult = await response.Content.ReadFromJsonAsync<GenAiProStatusResponse>(cancellationToken: cancellationToken);
            _logger.LogInformation("GenAiPro Task {TaskId} status (Attempt {Attempt}/{Max}): {Status}", 
                taskId, attempt, _settings.MaxPollingAttempts, statusResult?.Status);

            if (statusResult?.Status == "completed" && !string.IsNullOrEmpty(statusResult.Result))
            {
                _logger.LogInformation("GenAiPro Task {TaskId} completed! Downloading audio...", taskId);
                return await DownloadAudioAsync(statusResult.Result, cancellationToken);
            }

            if (statusResult?.Status == "failed")
            {
                _logger.LogError("GenAiPro Task {TaskId} failed on server side.", taskId);
                throw new Exception("GenAiPro audio generation task failed.");
            }
        }

        _logger.LogError("GenAiPro Task {TaskId} timed out after {Attempts} attempts.", taskId, _settings.MaxPollingAttempts);
        throw new Exception($"GenAiPro audio generation timed out after {_settings.MaxPollingAttempts * _settings.PollingIntervalMs / 1000} seconds.");
    }

    private async Task<Stream> DownloadAudioAsync(string audioUrl, CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, audioUrl);
        AddAuthHeaders(httpRequest);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("GenAiPro Download Audio Error: {StatusCode} - {Error}", response.StatusCode, error);
            throw new Exception($"Failed to download audio from GenAiPro: {response.StatusCode}");
        }

        var audioBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return new MemoryStream(audioBytes);
    }

    private void AddAuthHeaders(HttpRequestMessage request)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }
}
