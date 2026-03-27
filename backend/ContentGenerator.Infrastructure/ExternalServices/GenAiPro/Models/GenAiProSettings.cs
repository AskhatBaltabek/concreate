namespace ContentGenerator.Infrastructure.ExternalServices.GenAiPro.Models;

public class GenAiProSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://genaipro.vn/api/v1/labs";
    public string DefaultVoiceId { get; set; } = "dtSEyYGNJqjrtBArPCVZ";
    public string DefaultModelId { get; set; } = "eleven_v3";
    public double DefaultSpeed { get; set; } = 1.0;
    public double DefaultSimilarity { get; set; } = 0.75;
    public double DefaultStability { get; set; } = 0.5;
    public int MaxPollingAttempts { get; set; } = 180;
    public int PollingIntervalMs { get; set; } = 2000;
}
