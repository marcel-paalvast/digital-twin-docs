using DigitalTwin.Api.Constants;
using DigitalTwin.Api.Models;
using DigitalTwin.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.FeatureManagement;
using System.Text;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisDistributedCache("cache");
builder.AddAzureBlobService("blobs");

builder.Services.ConfigureHttpJsonOptions(options =>
{
});

builder.Services.AddOptions<ApiOptions>().Configure<IConfiguration>((settings, config) =>
{
    config.GetSection("Settings").Bind(settings);
});
builder.Services.AddOptions<OpenAiOptions>().Configure<IConfiguration>((settings, config) =>
{
    config.GetSection("OpenAi").Bind(settings);
});

builder.Services.AddSingleton<IMarkdownService, OpenAiMarkdownService>();
builder.Services.AddSingleton<IEnhanceMarkdownService, EnhanceMarkdownService>();
builder.Services.AddSingleton<IIPersistentStorageService, BlobPersistentStorageService>();

builder.Services.AddFeatureManagement();

var app = builder.Build();

app.MapDefaultEndpoints();

var markdownGroup = app.MapGroup("/markdown");

markdownGroup.MapGet("/{subject}.md", async (
    string subject,
    CancellationToken cancellationToken,
    [FromServices] IMarkdownService markdownService,
    [FromServices] IEnhanceMarkdownService enhanceMarkdownService,
    [FromServices] IDistributedCache distributedCache,
    [FromServices] IIPersistentStorageService persistentStorageService,
    [FromServices] IFeatureManager featureManager
    ) =>
{
    const int maxChars = 64;
    if (subject.Length > maxChars)
    {
        return Results.BadRequest(new ProblemDetails()
        {
            Title = $"Value exceeds allowed range",
            Detail = $"The value of {nameof(subject)} may not exceed {maxChars} characters",
        });
    }

    //get from cache
    if (await featureManager.IsEnabledAsync(FeatureFlags.CacheMarkdown))
    {
        var cachedMarkdown = await distributedCache.GetStringAsync(subject, cancellationToken);
        if (cachedMarkdown is not null)
        {
            return Results.Text(content: cachedMarkdown, contentType: "text/markdown", contentEncoding: Encoding.UTF8, statusCode: 200);
        }
    }

    //get from persistent storage
    if (await featureManager.IsEnabledAsync(FeatureFlags.PersistentStorage))
    {
        var storedMarkdown = await persistentStorageService.GetMarkdownAsync(subject, cancellationToken);
        if (storedMarkdown is not null)
        {
            return Results.Text(content: storedMarkdown, contentType: "text/markdown", contentEncoding: Encoding.UTF8, statusCode: 200);
        }
    }

    //generate new markdown
    var markdown = await markdownService.GenerateMarkdownAsync(subject, cancellationToken);
    markdown = enhanceMarkdownService.MarkAdditionalLinks(markdown);

    //store in cache
    if (await featureManager.IsEnabledAsync(FeatureFlags.CacheMarkdown))
    {
        var options = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
        };
        await distributedCache.SetStringAsync(subject, markdown, options, cancellationToken);
    }

    //store in persistent storage
    if (await featureManager.IsEnabledAsync(FeatureFlags.PersistentStorage))
    {
        await persistentStorageService.SetMarkdownAsync(subject, markdown, cancellationToken);
    }

    return Results.Text(content: markdown, contentType: "text/markdown", contentEncoding: Encoding.UTF8, statusCode: 200);
});

app.Run();