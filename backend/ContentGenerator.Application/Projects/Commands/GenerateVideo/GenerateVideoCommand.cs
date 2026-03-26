using MediatR;
namespace ContentGenerator.Application.Projects.Commands.GenerateVideo;

public record GenerateVideoCommand(Guid ProjectId) : IRequest;
