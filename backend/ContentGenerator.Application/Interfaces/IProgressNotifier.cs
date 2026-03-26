namespace ContentGenerator.Application.Interfaces;

/// <summary>
/// Service for broadcasting real-time progress events to connected SignalR clients.
/// </summary>
public interface IProgressNotifier
{
    Task NotifyProgressAsync(Guid projectId, string step, string status, string? message = null);
}
