// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace HeartBeatFunctionApp.Triggers;

public class HeartbeatEventGrid
{
    private readonly ILogger<HeartbeatEventGrid> _logger;

    public HeartbeatEventGrid(ILogger<HeartbeatEventGrid> logger)
    {
        _logger = logger;
    }

    [Function("HeartbeatEventGrid")]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        try
        {
            _logger.LogInformation("Event Grid trigger fired. Event: {eventType}, Subject: {subject}",
                eventGridEvent.EventType, eventGridEvent.Subject);

            // Deserialize data if needed
            var dataJson = eventGridEvent.Data.ToString();
            _logger.LogInformation("Event Data: {data}", dataJson);

            // TODO: implement logic (store in DB, log, etc.)
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in HeartbeatEventGrid function.");
            throw; // rethrow so Azure Functions shows failure
        }
    }
}