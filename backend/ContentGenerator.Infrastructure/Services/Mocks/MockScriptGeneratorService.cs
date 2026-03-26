using ContentGenerator.Application.Interfaces;

namespace ContentGenerator.Infrastructure.Services.Mocks;

public class MockScriptGeneratorService : IScriptGeneratorService
{
    public string Provider => "mock";

    public async Task<string> GenerateScriptAsync(string topic, string targetAudience, int lengthSeconds, CancellationToken cancellationToken)
    {
        // Simulate API delay
        await Task.Delay(2000, cancellationToken);
        
        return $@"
[SCENE 1]
Visual: Background abstract related to {topic}.
Voiceover: Have you ever wondered about {topic}? 

[SCENE 2]
Visual: Fast-paced transition with text pop-ups.
Voiceover: In this {lengthSeconds}-second video, we'll reveal everything for {targetAudience}!

[SCENE 3]
Visual: Subscribe button animation.
Voiceover: Like and subscribe for more!
";
    }
}
