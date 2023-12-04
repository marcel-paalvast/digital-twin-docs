using Azure.Messaging.ServiceBus;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DigitalTwin.Api.Services;

public partial class ServiceBusMessagingService : IMessagingService
{
    private readonly ServiceBusSender _sender;

    public ServiceBusMessagingService(ServiceBusClient client)
    {
        _sender = client.CreateSender("markdown");
    }

    public async Task PreloadLinksAsync(string markdown, CancellationToken cancellationToken)
    {
        var matches = SearchLinks().Matches(markdown);
        var subjects = matches
            .Select(x => x.Groups[1].Value);

        await PreloadAsync(subjects, cancellationToken);
    }

    private async Task PreloadAsync(IEnumerable<string> subjects, CancellationToken cancellationToken)
    {
        using ServiceBusMessageBatch batch = await _sender.CreateMessageBatchAsync(cancellationToken);

        foreach (var subject in subjects)
        {
            batch.TryAddMessage(new(subject));
        }

        await _sender.SendMessagesAsync(batch, cancellationToken);
    }

    [GeneratedRegex(@"\[[^\[\]]*?\]\(([^\(\)]*?).md\)", RegexOptions.Multiline)]
    private static partial Regex SearchLinks();
}
