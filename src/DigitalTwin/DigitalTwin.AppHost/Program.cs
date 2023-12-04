var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedisContainer("cache");
var blobs = builder.AddAzureStorage("storage")
    .AddBlobs("Blobs");
var serviceBus = builder.AddAzureServiceBus("messaging");

var api = builder.AddProject<Projects.DigitalTwin_Api>("digitaltwin.api")
    .WithReference(cache)
    .WithReference(blobs)
    .WithReference(serviceBus);

builder.AddProject<Projects.DigitalTwin_Blazor>("digitaltwin.blazor")
    .WithReference(api);

builder.AddProject<Projects.DigitalTwin_Worker>("digitaltwin.worker")
    .WithReference(blobs)
    .WithReference(serviceBus);

builder.Build().Run();