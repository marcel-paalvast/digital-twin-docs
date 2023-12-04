
namespace DigitalTwin.Api.Services;

public interface IIPersistentStorageService
{
    Task<string?> GetMarkdownAsync(string subject, CancellationToken cancellationToken);
    Task SetMarkdownAsync(string subject, string content, CancellationToken cancellationToken);
}