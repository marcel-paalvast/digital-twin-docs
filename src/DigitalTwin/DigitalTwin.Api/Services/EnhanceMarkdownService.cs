using System.Text.RegularExpressions;

namespace DigitalTwin.Api.Services;

public partial class EnhanceMarkdownService : IEnhanceMarkdownService
{
    /// <summary>
    /// Searches for potential links and converts them to markdown link format.
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    public string MarkAdditionalLinks(string markdown)
    {
        static string evaluator(Match m)
        {
            var group2 = m.Groups[2].Value;
            var noParenthesis = RemoveParenthesis(group2);
            var normalized = RemoveDiacritics(noParenthesis);
            var link = FindNonLowercaseAlphabeticalText().Replace(normalized.ToLowerInvariant(), "-").Trim('-');
            return $"{m.Groups[1].Value}[{group2}]({link}.md){m.Groups[4].Value}{m.Groups[3].Value}";
        }

        return FindMarkdownBulletPointHeaders().Replace(markdown, evaluator);
    }

    static string RemoveParenthesis(string input) =>
        FindParenthesizedText().Replace(input, "");

    static string RemoveDiacritics(string input) =>
        FindDiacritics().Replace(input.Normalize(System.Text.NormalizationForm.FormD), "");

    [GeneratedRegex(@"^\s*((?:[0-9]+\.|-)\s+)((?:\*\*)?[A-Za-z][^\.]{0,64}?)(:(\*\*)?|\r?\n)", RegexOptions.Multiline)]
    private static partial Regex FindMarkdownBulletPointHeaders();
    [GeneratedRegex(@"\p{M}")]
    private static partial Regex FindDiacritics();
    [GeneratedRegex(@"[^a-z]+")]
    private static partial Regex FindNonLowercaseAlphabeticalText();
    [GeneratedRegex(@" ?\([^\(]*?\)")]
    private static partial Regex FindParenthesizedText();
}