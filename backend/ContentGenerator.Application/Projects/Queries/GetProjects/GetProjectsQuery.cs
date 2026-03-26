using MediatR;
using ContentGenerator.Application.Projects.Queries.GetProject; 

namespace ContentGenerator.Application.Projects.Queries.GetProjects;

public record GetProjectsQuery(string UserId) : IRequest<List<ProjectDto>>;
