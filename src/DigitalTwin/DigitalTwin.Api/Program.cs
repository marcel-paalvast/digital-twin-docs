using DigitalTwin.Api.Models;
using DigitalTwin.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.AddServiceDefaults();

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

builder.Services.AddFeatureManagement();

var app = builder.Build();

app.MapDefaultEndpoints();

var markdownGroup = app.MapGroup("/markdown");

markdownGroup.MapGet("/{subject}.md", async (
    string subject,
    CancellationToken cancellationToken,
    [FromServices] IMarkdownService markdownService,
    [FromServices] IEnhanceMarkdownService enhanceMarkdownService
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

    var markdown = await markdownService.GenerateMarkdownAsync(subject, cancellationToken);
    markdown = enhanceMarkdownService.MarkAdditionalLinks(markdown);
    return Results.Text(content: markdown, contentType: "text/markdown", contentEncoding: Encoding.UTF8, statusCode: 200);
});

app.Run();