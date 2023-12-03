using DigitalTwin.Blazor;
using DigitalTwin.Blazor.Components;
using DigitalTwin.Blazor.Models;
using DigitalTwin.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOptions<PageOptions>().Configure<IConfiguration>((settings, config) =>
{
    config.GetSection("Settings").Bind(settings);
});

builder.Services.AddScoped<IMarkdownService, MarkdownService>();
builder.Services.AddScoped<IDocumentationService, DocumentationService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

