using DigitalTwin.Api.Models;
using DigitalTwin.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    //options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
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


var app = builder.Build();

var markdownGroup = app.MapGroup("/markdown");
markdownGroup.MapGet("/{subject}.md", async (string subject, CancellationToken cancellationToken, [FromServices] IMarkdownService service) =>
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

    var markdown = await service.GenerateMarkdownAsync(subject, cancellationToken);
    return Results.Text(content: markdown, contentType: "text/markdown", contentEncoding: Encoding.UTF8, statusCode: 200);
});

app.Run();

//public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

//[JsonSerializable(typeof(Todo[]))]
//internal partial class AppJsonSerializerContext : JsonSerializerContext
//{

//}
