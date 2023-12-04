using Azure.Messaging.ServiceBus;
using DigitalTwin.Api.Services;

namespace DigitalTwin.Worker;

public class Worker(
    ILogger<Worker> logger,
    ServiceBusClient client,
    IEnhanceMarkdownService enhanceMarkdownService,
    IMarkdownService markdownService,
    IIPersistentStorageService persistentStorageService
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var processor = client.CreateProcessor("markdown", "worker");

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(int.MaxValue, cancellationToken);
            }

            await processor.StopProcessingAsync(cancellationToken);
        }
    }

    async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string subject = args.Message.Body.ToString();

        if (await persistentStorageService.MarkdownExistsAsync(subject, args.CancellationToken))
        {
            logger.LogInformation("Skipped {subject}, already exists", subject);
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        var markdown = await markdownService.GenerateMarkdownAsync(subject, args.CancellationToken);
        markdown = enhanceMarkdownService.MarkAdditionalLinks(markdown);
        await persistentStorageService.SetMarkdownAsync(subject, markdown, args.CancellationToken);

        logger.LogInformation("Processed {subject}", subject);
        await args.CompleteMessageAsync(args.Message);
    }

    Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError("Exception occurred: {message}", args.Exception.Message);
        return Task.CompletedTask;
    }
}