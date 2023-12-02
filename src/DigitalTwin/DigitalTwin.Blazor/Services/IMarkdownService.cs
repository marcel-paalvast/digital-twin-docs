
namespace DigitalTwin.Blazor.Services;

public interface IMarkdownService
{
    Task<string> GetMarkdownAsync(string subject, CancellationToken cancellationToken);
}