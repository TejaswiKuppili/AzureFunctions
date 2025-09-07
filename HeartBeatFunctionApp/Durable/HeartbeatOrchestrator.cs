using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HeartBeatFunctionApp.Durable;

public class HeartbeatOrchestrator
{
    [Function(nameof(HeartbeatOrchestrator))]
    public async Task RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<HeartbeatOrchestrator>();
        logger.LogInformation("Orchestration started at {time}", context.CurrentUtcDateTime);

        // Call activity function
        string result = await context.CallActivityAsync<string>(
            nameof(ActivityFunction), "Tejaswi");

        logger.LogInformation("Orchestration received: {msg}", result);

        // Example: wait 5 seconds and call again
        await context.CreateTimer(context.CurrentUtcDateTime.AddSeconds(5), default);

        string secondResult = await context.CallActivityAsync<string>(
            nameof(ActivityFunction), "Azure");

        logger.LogInformation("Orchestration finished with: {msg}", secondResult);
    }
}
