using Microsoft.AspNetCore.SignalR;

namespace ContentGenerator.Api.Hubs;

/// <summary>
/// SignalR hub for streaming real-time generation progress to connected clients.
/// </summary>
public class VideoGenerationHub : Hub
{
    /// <summary>
    /// Client calls this to subscribe to a specific project's updates.
    /// </summary>
    public async Task JoinProject(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
    }

    /// <summary>
    /// Client can call this to unsubscribe.
    /// </summary>
    public async Task LeaveProject(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
    }
}
