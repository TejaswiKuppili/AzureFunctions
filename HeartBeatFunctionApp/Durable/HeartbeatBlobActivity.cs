using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HeartBeatFunctionApp.Durable;

public class ActivityFunction
{
    private readonly ILogger _logger;

    public ActivityFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ActivityFunction>();
    }

    [Function(nameof(ActivityFunction))]
    public Task<string> RunAsync([ActivityTrigger] string name)
    {
        _logger.LogInformation("Activity executed with input: {name}", name);
        return Task.FromResult($"Hello, {name}!");
    }
}
