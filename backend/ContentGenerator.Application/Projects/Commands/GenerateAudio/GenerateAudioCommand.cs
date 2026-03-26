using MediatR;
namespace ContentGenerator.Application.Projects.Commands.GenerateAudio;

public record GenerateAudioCommand(Guid ProjectId, string ScriptText, string? VoiceModelId = null, string? Provider = "genaipro") : IRequest<string>;
