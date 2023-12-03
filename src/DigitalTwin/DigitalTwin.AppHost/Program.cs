var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedisContainer("cache");

var api = builder.AddProject<Projects.DigitalTwin_Api>("digitaltwin.api")
    .WithReference(cache);

builder.AddProject<Projects.DigitalTwin_Blazor>("digitaltwin.blazor")
    .WithReference(api);

builder.Build().Run();
