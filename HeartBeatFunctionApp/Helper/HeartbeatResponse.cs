using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace HeartBeatFunctionApp.Helper
{
    public class HeartbeatResponse
    {
        [QueueOutput("heartbeat-queue", Connection = "AzureWebJobsStorage")]
        public string? QueueMessage { get; set; }

        [BlobOutput("heartbeats/{rand-guid}.json", Connection = "AzureWebJobsStorage")]
        public string? BlobContent { get; set; }

        public HttpResponseData? HttpResponse { get; set; }
    }
}
