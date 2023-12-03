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
builder.Services.AddSingleton<ICacheService, DistributedCacheService>();

builder.Services.AddFeatureManagement();

var app = builder.Build();

app.MapDefaultEndpoints();

var markdownGroup = app.MapGroup("/markdown");

markdownGroup.MapGet("/{subject}.md", async (
    string subject,
    CancellationToken cancellationToken,
    [FromServices] IMarkdownService markdownService,
    [FromServices] IEnhanceMarkdownService enhanceMarkdownService,
    [FromServices] ICacheService cacheService,
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

    string? markdown = null;
    var isCached = false;
    var isGenerated = false;

    //get from cache
    if (markdown is null && await featureManager.IsEnabledAsync(FeatureFlags.CacheMarkdown))
    {
        markdown = await cacheService.GetMarkdownAsync(subject, cancellationToken);
        if (markdown is not null)
        {
            isCached = true;
        }
    }

    //get from persistent storage
    if (markdown is null && await featureManager.IsEnabledAsync(FeatureFlags.PersistentStorage))
    {
        markdown = await persistentStorageService.GetMarkdownAsync(subject, cancellationToken);
    }

    //generate new markdown
    if (markdown is null)
    {
        markdown = await markdownService.GenerateMarkdownAsync(subject, cancellationToken);
        markdown = enhanceMarkdownService.MarkAdditionalLinks(markdown);
        isGenerated = true;
    }

    var tasks = new List<Task>();

    //store in cache
    if (!isCached && await featureManager.IsEnabledAsync(FeatureFlags.CacheMarkdown))
    {
        tasks.Add(cacheService.SetMarkdownAsync(subject, markdown, cancellationToken));
    }

    //store in persistent storage
    if (isGenerated && await featureManager.IsEnabledAsync(FeatureFlags.PersistentStorage))
    {
        tasks.Add(persistentStorageService.SetMarkdownAsync(subject, markdown, cancellationToken));
    }

    await Task.WhenAll(tasks);

    return Results.Text(content: markdown, contentType: "text/markdown", contentEncoding: Encoding.UTF8, statusCode: 200);
});

app.Run();