using Markdig;

namespace DigitalTwin.Blazor.Services;

public class DocumentationService(IMarkdownService markdownService) : IDocumentationService
{
    private readonly IMarkdownService _markdownService = markdownService;
    private readonly MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder()
        .UseAutoIdentifiers()
        .UseCitations()
        .UseFootnotes()
        .Build();

    public async Task<string> GetDocumentationHtmlAsync(string subject, CancellationToken cancellationToken)
    {
        var markdown = await _markdownService.GetMarkdownAsync(subject, cancellationToken);
        return Markdown.ToHtml(markdown, markdownPipeline);
    }
}
