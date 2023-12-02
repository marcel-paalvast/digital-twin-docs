namespace DigitalTwin.Api.Models;

public record ApiOptions
{
    public required Company Company { get; set; }
}

public record Company
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}