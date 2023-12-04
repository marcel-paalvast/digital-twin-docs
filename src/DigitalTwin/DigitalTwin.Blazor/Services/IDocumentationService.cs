
namespace DigitalTwin.Blazor.Services;

public interface IDocumentationService
{
    Task<string> GetDocumentationHtmlAsync(string subject, CancellationToken cancellationToken);
}