using DigitalTwin.Api.Models;
using DigitalTwin.Api.Services;
using DigitalTwin.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.AddAzureBlobService("blobs");
builder.AddAzureServiceBus("messaging");

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

var host = builder.Build();
host.Run();
