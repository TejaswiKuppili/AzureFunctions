using HeartBeatFunctionApp.Helper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace HeartBeatFunctionApp;

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

        // queue message
        var message = $"Heartbeat recorded at {_lastHeartbeat:o}";
        var blobJson = JsonSerializer.Serialize(new
        {
            recordedAt = _lastHeartbeat,
            source = "HTTP"
        });

        var created = req.CreateResponse(HttpStatusCode.Created);
        await created.WriteStringAsync("Heartbeat recorded and queued.");

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