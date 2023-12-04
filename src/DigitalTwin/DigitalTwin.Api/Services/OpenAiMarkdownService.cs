using Azure.AI.OpenAI;
using Azure;
using DigitalTwin.Api.Models;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace DigitalTwin.Api.Services;

public record OpenAiOptions
{
    public required string Uri { get; set; }
    public required string Key { get; set; }
}

public class OpenAiMarkdownService : IMarkdownService
{
    private readonly OpenAIClient _client;
    private readonly Company _company;
    const string Deployment = "gpt-3.5-turbo";

    public OpenAiMarkdownService(
        IOptions<OpenAiOptions> openAiOptions,
        IOptions<ApiOptions> apiOptions)
    {
        Uri uri = new(openAiOptions.Value.Uri);
        AzureKeyCredential credential = new(openAiOptions.Value.Key);
        _client = new(uri, credential);
        _company = apiOptions.Value.Company;
    }

    public async Task<string> GenerateMarkdownAsync(string subject, CancellationToken cancellationToken)
    {
        var instructions = $"""
            You are tasked to create internal documentation for a company named "{_company.Name}" which "{_company.Description}".
            In the documentation describe the entire organization and all of its aspects which will be used as guidance to direct the company.
            USE markdown format
            USE links to other markdown pages using: [key words](example-page.md)
            DO not include a table of contents
            Be detailed
            Include both paragraphs of text and bullet points that start with keywords
            Do NOT explain yourself or the answer
            Do NOT use placeholders
            """;
        var question = $"""
            You are currently tasked to make a page specifically for the subject "{subject}".
            """;

        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = Deployment,
            Messages =
            {
                new ChatMessage(ChatRole.System, instructions),
                new ChatMessage(ChatRole.User, question),
            }
        };

        var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
        return response.Value.Choices[0].Message.Content;
    }
}
