using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.ComponentModel;

namespace DigitalTwin.Api.Services;

public class BlobPersistentStorageService(BlobServiceClient client) : IIPersistentStorageService
{
    private readonly BlobContainerClient _container = client.GetBlobContainerClient("markdown");
    private readonly BlobUploadOptions _uploadOptions = new()
    {
        Conditions = new()
        {
            //only allow new uploads
            IfNoneMatch = ETag.All,
        }
    };

    public async Task<string?> GetMarkdownAsync(string subject, CancellationToken cancellationToken)
    {
        var blob = _container.GetBlobClient(GetBlobName(subject));
        try
        {
            var response = await blob.DownloadContentAsync(cancellationToken);
            return response.Value.Content.ToString();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            //blob does not exist
            return null;
        }
    }

    public async Task SetMarkdownAsync(string subject, string content, CancellationToken cancellationToken)
    {
        var blob = _container.GetBlobClient(GetBlobName(subject));
        var data = BinaryData.FromString(content);
        try
        {
            var response = await blob.UploadAsync(data, _uploadOptions, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            //blob already exists
            //do nothing
        }
    }

    public async Task<bool> MarkdownExistsAsync(string subject, CancellationToken cancellationToken)
    {
        var blob = _container.GetBlobClient(GetBlobName(subject));
        return await blob.ExistsAsync(cancellationToken);
    }

    private static string GetBlobName(string subject) => $"{subject}.md";
}
