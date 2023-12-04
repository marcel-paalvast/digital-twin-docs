var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedisContainer("cache");
var blobs = builder.AddAzureStorage("storage")
    .AddBlobs("Blobs");

var api = builder.AddProject<Projects.DigitalTwin_Api>("digitaltwin.api")
    .WithReference(cache)
    .WithReference(blobs);

builder.AddProject<Projects.DigitalTwin_Blazor>("digitaltwin.blazor")
    .WithReference(api);

builder.Build().Run();
