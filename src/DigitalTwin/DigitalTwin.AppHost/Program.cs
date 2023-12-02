var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DigitalTwin_Api>("digitaltwin.api");

builder.Build().Run();
