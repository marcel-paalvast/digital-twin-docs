using Microsoft.Extensions.Caching.Distributed;

namespace DigitalTwin.Api.Services;

public class DistributedCacheService(IDistributedCache distributedCache) : ICacheService
{
    public async Task<string?> GetMarkdownAsync(string subject, CancellationToken cancellationToken) =>
        await distributedCache.GetStringAsync(subject, cancellationToken);

    public async Task SetMarkdownAsync(string subject, string content, CancellationToken cancellationToken)
    {
        var options = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
        };
        await distributedCache.SetStringAsync(subject, content, options, cancellationToken);
    }
}
