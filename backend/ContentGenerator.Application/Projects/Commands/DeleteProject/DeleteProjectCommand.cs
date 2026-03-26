using MediatR;

namespace ContentGenerator.Application.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid ProjectId, Guid UserId) : IRequest<bool>;
