# HeartBeatFunction

The Heartbeat Function App is a serverless application built using Azure Functions (.NET Isolated Worker).
It demonstrates different Azure Function Triggers working together in a real-time heartbeat monitoring system:

HTTP Trigger → Records a heartbeat from API calls

Timer Trigger → Records periodic heartbeat automatically

Queue Trigger → Processes heartbeat messages from Azure Storage Queues

Blob Trigger → Reacts when new blobs are added and downloads them automatically

⚙️ Prerequisites

1. Azure Subscription
2. Azure Storage Account
   * With Queue and Blob services enabled
3. Visual Studio 2022 (latest) with Azure workload
4. Azure Functions Core Tools (for local testing)

**Implemented Triggers**

1️⃣ HTTP Trigger – HeartbeatHttp

Endpoint:

GET /api/heartbeat → Returns last recorded heartbeat

POST /api/heartbeat → Records new heartbeat

Actions:

Saves timestamp

Pushes message into heartbeat-queue

Writes blob into heartbeats/ container

2️⃣ Timer Trigger – TimerHeartbeat

CRON schedule:

[TimerTrigger("0 */10 * * * *")] // Every 10 minutes


Automatically records heartbeat at scheduled intervals

Logs timestamp in Application Insights

3️⃣ Queue Trigger – HeartbeatQueueProcessor

Watches heartbeat-queue

Triggered when new messages arrive

Example action:

Logs received message

(Can be extended: email, DB insert, notifications, etc.)

4️⃣ Blob Trigger – HeartbeatBlobProcessor

Watches heartbeats/ blob container

Triggered when a new blob is added

Downloads blob automatically to local Downloads folder
