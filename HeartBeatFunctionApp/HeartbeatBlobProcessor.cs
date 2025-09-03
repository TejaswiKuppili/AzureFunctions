using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace HeartBeatFunctionApp;

public class HeartbeatBlobProcessor
{
    private readonly ILogger _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public HeartbeatBlobProcessor(ILoggerFactory loggerFactory, BlobServiceClient blobServiceClient)
    {
        _logger = loggerFactory.CreateLogger<HeartbeatBlobProcessor>();
        _blobServiceClient = blobServiceClient;
    }

    [Function("HeartbeatBlobProcessor")]
    public async Task Run(
        [BlobTrigger("heartbeats/{name}", Connection = "AzureWebJobsStorage")] string blobContent,
        string name)
    {
        _logger.LogInformation("Blob trigger fired for blob: {name}", name);

        // ✅ Downloads folder path (works locally)
        string downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");

        // Ensure folder exists
        Directory.CreateDirectory(downloadsPath);

        string localPath = Path.Combine(downloadsPath, name);

        var containerClient = _blobServiceClient.GetBlobContainerClient("heartbeats");
        var blobClient = containerClient.GetBlobClient(name);

        await blobClient.DownloadToAsync(localPath);

        _logger.LogInformation("Blob {name} downloaded locally to {path}", name, localPath);
    }
}
