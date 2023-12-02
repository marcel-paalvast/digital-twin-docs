using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DigitalTwin.Blazor.Services;

public class MarkdownService(HttpClient client) : IMarkdownService
{
    private readonly HttpClient _client = client;

    public async Task<string> GetMarkdownAsync(string subject, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"http://digitaltwin.api/markdown/{subject}.md", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Invalid response code: {response.StatusCode}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
