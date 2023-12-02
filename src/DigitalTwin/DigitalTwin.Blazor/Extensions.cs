namespace DigitalTwin.Blazor;

public static class Extensions
{
    public static IHostApplicationBuilder AddCustomServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler(c =>
            {
                c.CircuitBreaker = new()
                {
                    SamplingDuration = TimeSpan.FromSeconds(60),
                };
                c.AttemptTimeout = new()
                {
                    //increased timeout for chatgpt
                    Timeout = TimeSpan.FromSeconds(30),

                };
            });

            // Turn on service discovery by default
            http.UseServiceDiscovery();
        });

        return builder;
    }
}
