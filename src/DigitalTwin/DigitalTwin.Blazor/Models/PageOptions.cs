namespace DigitalTwin.Blazor.Models;

public record PageOptions
{
    public required Company Company { get; set; }
}

public record Company
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}
