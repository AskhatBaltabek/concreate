using ContentGenerator.Application.Projects.Commands.CreateProject;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ContentGenerator.Api.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects").RequireAuthorization();

        group.MapGet("/", async (IMediator mediator, ClaimsPrincipal user) =>
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Results.Unauthorized();
            var projects = await mediator.Send(new ContentGenerator.Application.Projects.Queries.GetProjects.GetProjectsQuery(userIdString));
            return Results.Ok(projects);
        });

        group.MapPost("/", async (CreateProjectRequest req, IMediator mediator, ClaimsPrincipal user) =>
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId)) return Results.Unauthorized();

            var command = new CreateProjectCommand(req.Title, req.Description, req.LengthSeconds, req.Format, userId);
            var projectId = await mediator.Send(command);

            return Results.Ok(new { ProjectId = projectId });
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var project = await mediator.Send(new ContentGenerator.Application.Projects.Queries.GetProject.GetProjectQuery(id));
            return Results.Ok(project);
        });

        group.MapPost("/{id:guid}/script", async (Guid id, string? provider, IMediator mediator) =>
        {
            var script = await mediator.Send(new ContentGenerator.Application.Projects.Commands.GenerateScript.GenerateScriptCommand(id, provider));
            return Results.Ok(new { Script = script });
        });

        group.MapPost("/{id:guid}/audio", async (Guid id, string? provider, [Microsoft.AspNetCore.Mvc.FromBody] GenerateAudioRequest req, IMediator mediator) =>
        {
            var url = await mediator.Send(new ContentGenerator.Application.Projects.Commands.GenerateAudio.GenerateAudioCommand(id, req.ScriptText, req.VoiceModelId, provider));
            return Results.Ok(new { AudioUrl = url });
        });

        group.MapPost("/{id:guid}/video", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new ContentGenerator.Application.Projects.Commands.GenerateVideo.GenerateVideoCommand(id));
            return Results.Accepted();
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, ClaimsPrincipal user) =>
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId)) return Results.Unauthorized();

            var deleted = await mediator.Send(new ContentGenerator.Application.Projects.Commands.DeleteProject.DeleteProjectCommand(id, userId));
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}

public record GenerateAudioRequest(string ScriptText, string? VoiceModelId = null);

public record CreateProjectRequest(string Title, string Description, int LengthSeconds = 30, string Format = "9:16");
