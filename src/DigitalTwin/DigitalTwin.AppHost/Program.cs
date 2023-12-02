var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.DigitalTwin_Api>("digitaltwin.api");

builder.AddProject<Projects.DigitalTwin_Blazor>("digitaltwin.blazor")
    .WithReference(api);

builder.Build().Run();
