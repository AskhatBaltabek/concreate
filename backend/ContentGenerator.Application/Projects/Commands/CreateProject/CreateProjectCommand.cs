using MediatR;

namespace ContentGenerator.Application.Projects.Commands.CreateProject;

public record CreateProjectCommand(string Title, string Description, int LengthSeconds, string Format, Guid UserId) : IRequest<Guid>;
