using System.Text;
using System.Text.RegularExpressions;

namespace ContentGenerator.Application.Utils;

public static class ScriptParser
{
    /// <summary>
    /// Extracts voiceover text from a structured script that uses [VOICE], Voiceover: "text", or similar patterns.
    /// </summary>
    public static string ExtractVoiceOver(string script)
    {
        if (string.IsNullOrWhiteSpace(script)) return string.Empty;

        var sb = new StringBuilder();
        
        // Pattern 1: Voiceover: "..." (case insensitive)
        var voiceoverRegex = new Regex(@"Voiceover:\s*""([^""]*)""", RegexOptions.IgnoreCase);
        var matches = voiceoverRegex.Matches(script);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var text = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append(text);
                    sb.Append(" ");
                }
            }
        }

        // Pattern 2: [VOICE]...[/VOICE] (as a secondary check)
        if (sb.Length == 0)
        {
            var voiceTagRegex = new Regex(@"\[VOICE\](.*?)\[/VOICE\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var tagMatches = voiceTagRegex.Matches(script);
            foreach (Match match in tagMatches)
            {
                var text = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append(text);
                    sb.Append(" ");
                }
            }
        }

        // Fallback: If no patterns found, return original script? 
        // No, user probably wants us to be careful. But for now let's return the cleaned string.
        var result = sb.ToString().Trim();
        
        // If we found nothing with specific regex, but the script looks like plain text, 
        // maybe it's already a clean VO. But let's stick to the extracted part for now.
        return string.IsNullOrEmpty(result) ? "" : result;
    }
}
