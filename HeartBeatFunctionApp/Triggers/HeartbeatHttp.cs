using Azure;
using Azure.Messaging.EventGrid;
using HeartBeatFunctionApp.Helper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace HeartBeatFunctionApp.Triggers;

public class HeartbeatHttp
{
    private readonly ILogger _logger;
    private static DateTime? _lastHeartbeat;

    public HeartbeatHttp(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HeartbeatHttp>();
    }

    [Function("HeartbeatHttp")]
    public async Task<HeartbeatResponse> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "heartbeat")] HttpRequestData req)
    {
        var result = new HeartbeatResponse();

        if (req.Method == "GET")
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(JsonSerializer.Serialize(new { last = _lastHeartbeat?.ToString("o") }));

            return new HeartbeatResponse
            {
                QueueMessage = null,
                BlobContent = null,
                HttpResponse = response
            };
        }

        // POST -> record heartbeat
        _lastHeartbeat = DateTime.UtcNow;
        _logger.LogInformation("HTTP heartbeat recorded at {time}", _lastHeartbeat);

        // Queue - Blob
        var message = $"Heartbeat recorded at {_lastHeartbeat:o}";
        var blobJson = JsonSerializer.Serialize(new
        {
            recordedAt = _lastHeartbeat,
            source = "HTTP"
        });

        // Publish event to Event Grid
        try
        {
            string? topicEndpoint = Environment.GetEnvironmentVariable("EventGridTopicEndpoint");
            string? topicKey = Environment.GetEnvironmentVariable("EventGridAccessKey");

            if (string.IsNullOrEmpty(topicEndpoint) || string.IsNullOrEmpty(topicKey))
            {
                throw new InvalidOperationException("Event Grid configuration is missing.");
            }

            var credentials = new AzureKeyCredential(topicKey);
            var client = new EventGridPublisherClient(new Uri(topicEndpoint), credentials);

            var eventPayload = new EventGridEvent(
                subject: "heartbeat/new",
                eventType: "HeartbeatRecorded",
                dataVersion: "1.0",
                data: new { timestamp = _lastHeartbeat, source = "HeartbeatHttp" }
            );

            await client.SendEventAsync(eventPayload);
            _logger.LogInformation("Event Grid event published successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish Event Grid event.");
        }

        //string topicEndpoint = Environment.GetEnvironmentVariable("EventGridTopicEndpoint")!;
        //string topicKey = Environment.GetEnvironmentVariable("EventGridAccessKey")!;

        //var credentials = new AzureKeyCredential(topicKey);
        //var client = new EventGridPublisherClient(new Uri(topicEndpoint), credentials);

        //var eventPayload = new EventGridEvent(
        //    subject: "heartbeat/new",
        //    eventType: "HeartbeatRecorded",
        //    dataVersion: "1.0",
        //    data: new { timestamp = _lastHeartbeat, source = "HeartbeatHttp" }
        //);

        //await client.SendEventAsync(eventPayload);

        var created = req.CreateResponse(HttpStatusCode.Created);
        await created.WriteStringAsync("Heartbeat recorded, queued and published to Event Grid.");

        return new HeartbeatResponse
        {
            QueueMessage = message,
            BlobContent = blobJson,
            HttpResponse = created
        };
    }

    //[Function("HeartbeatHttp")]
    //public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "heartbeat")] HttpRequestData req)
    //{
    //    if (req.Method == "GET")
    //    {
    //        var response = req.CreateResponse(HttpStatusCode.OK);
    //        await response.WriteStringAsync(JsonSerializer.Serialize(new { last = _lastHeartbeat?.ToString("o") }));
    //        return response;
    //    }

    //    // POST -> record heartbeat
    //    _lastHeartbeat = DateTime.UtcNow;
    //    _logger.LogInformation("HTTP heartbeat recorded at {time}", _lastHeartbeat);

    //    var created = req.CreateResponse(HttpStatusCode.Created);
    //    await created.WriteStringAsync("Heartbeat recorded");
    //    return created;
    //}
}