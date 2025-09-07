//Have to comment the Blob Trigger to avoid duplicate processing as this branch
//implements Durable orchestration functions on top of Blob trigger
 


using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace HeartBeatFunctionApp.Triggers;

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

        var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

        if (env == "Development")
        {
            // ✅ Local mode → save to real Downloads folder
            string downloadsPath = @"C:\Users\Tejaswi Kuppili\Downloads";
            Directory.CreateDirectory(downloadsPath);

            string localPath = Path.Combine(downloadsPath, name);

            var containerClient = _blobServiceClient.GetBlobContainerClient("heartbeats");
            var blobClient = containerClient.GetBlobClient(name);

            await blobClient.DownloadToAsync(localPath);

            _logger.LogInformation("Blob {name} downloaded locally to {path}", name, localPath);
        }
        else
        {
            // Azure mode → copy blob into another container
            var processedContainer = _blobServiceClient.GetBlobContainerClient("heartbeats");

            var processedBlobClient = processedContainer.GetBlobClient(name);

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(blobContent));
            await processedBlobClient.UploadAsync(stream, overwrite: true);

            _logger.LogInformation("Blob {name} copied to heartbeats container", name);
        }

    }
}
