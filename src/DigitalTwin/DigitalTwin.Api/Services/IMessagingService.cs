
namespace DigitalTwin.Api.Services;

public interface IMessagingService
{
    Task PreloadLinksAsync(string markdown, CancellationToken cancellationToken);
}