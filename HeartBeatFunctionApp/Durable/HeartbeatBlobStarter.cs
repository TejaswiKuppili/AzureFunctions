using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HeartBeatFunctionApp.Durable;

public class HeartbeatBlobStarter
{
    private readonly ILogger _logger;

    public HeartbeatBlobStarter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HeartbeatBlobStarter>();
    }

    [Function("HeartbeatBlobStarter")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(HeartbeatOrchestrator));

        _logger.LogInformation("Started orchestration with ID = {id}", instanceId);

        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}
