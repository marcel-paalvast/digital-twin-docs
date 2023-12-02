namespace DigitalTwin.Api.Services;

public interface IMarkdownService
{
    Task<string> GenerateMarkdownAsync(string subject, CancellationToken cancellationToken);
}