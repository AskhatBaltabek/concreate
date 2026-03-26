using MediatR;
namespace ContentGenerator.Application.Projects.Queries.GetProject;

public record GetProjectQuery(Guid ProjectId) : IRequest<ProjectDto>;
public record ProjectDto(Guid Id, string Title, string Description, int Status, string? GeneratedScript, string? AudioUrl, string? VideoUrl);
