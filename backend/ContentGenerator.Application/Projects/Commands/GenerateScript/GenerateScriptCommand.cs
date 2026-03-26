using MediatR;
namespace ContentGenerator.Application.Projects.Commands.GenerateScript;

public record GenerateScriptCommand(Guid ProjectId, string? Provider = null) : IRequest<string>;
