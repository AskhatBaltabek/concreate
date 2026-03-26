using ContentGenerator.Application.Interfaces;
using ContentGenerator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContentGenerator.Application.Projects.Commands.GenerateScript;

public class GenerateScriptCommandHandler : IRequestHandler<GenerateScriptCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly IEnumerable<IScriptGeneratorService> _scriptGenerators;

    public GenerateScriptCommandHandler(IAppDbContext context, IEnumerable<IScriptGeneratorService> scriptGenerators)
    {
        _context = context;
        _scriptGenerators = scriptGenerators;
    }

    public async Task<string> Handle(GenerateScriptCommand request, CancellationToken cancellationToken)
    {
        var provider = request.Provider?.ToLower() ?? "claude";
        var scriptGenerator = _scriptGenerators.FirstOrDefault(g => g.Provider == provider) 
                             ?? _scriptGenerators.FirstOrDefault(g => g.Provider == "claude")
                             ?? _scriptGenerators.First();

        var project = await _context.Projects
            .Include(p => p.Settings)
            .Include(p => p.Script)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
            
        if (project == null) throw new Exception("Project not found");

        var generatedScript = await scriptGenerator.GenerateScriptAsync(
            project.Title + " - " + project.Description, 
            project.Settings.TargetAudience, 
            project.Settings.LengthSeconds, 
            cancellationToken);

        var voiceOverText = ContentGenerator.Application.Utils.ScriptParser.ExtractVoiceOver(generatedScript);

        if (project.Script == null)
        {
            project.Script = new ScriptLayer
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                RawPrompt = "Default system prompt for " + project.Title,
                GeneratedScript = generatedScript,
                VoiceOverText = voiceOverText,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Scripts.Add(project.Script);
        }
        else
        {
            project.Script.GeneratedScript = generatedScript;
            project.Script.VoiceOverText = voiceOverText;
            project.Script.UpdatedAt = DateTime.UtcNow;
        }

        project.Status = ProjectStatus.UserReview; 

        await _context.SaveChangesAsync(cancellationToken);

        return generatedScript;
    }
}
