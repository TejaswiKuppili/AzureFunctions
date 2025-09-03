using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

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
