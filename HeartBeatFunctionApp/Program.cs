using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.DurableTask.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register Durable Task extension (no HubName here)
builder.Services.AddDurableTaskWorker(options =>
{
    //options.UseStorage(new AzureStorageDurabilityProviderFactory());
    options.UseGrpc();
});

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register BlobServiceClient using your storage connection string
builder.Services.AddSingleton(sp =>
{
    var storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    return new BlobServiceClient(storageConn);
});

builder.Build().Run();
