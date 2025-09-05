using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;

namespace HeartBeatFunctionApp.Triggers;

public class HeartbeatQueueProcessor
{
    private readonly ILogger _logger;

    public HeartbeatQueueProcessor(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HeartbeatQueueProcessor>();
    }

    [Function("HeartbeatQueueProcessor")]
    public async Task Run(
        [QueueTrigger("heartbeat-queue", Connection = "AzureWebJobsStorage")] string queueMessage)
    {
        _logger.LogInformation($"Processing heartbeat message: {queueMessage}");

        // Load config from environment
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var fromEmail = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL");
        var toEmail = Environment.GetEnvironmentVariable("SENDGRID_TO_EMAIL");

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(toEmail))
        {
            _logger.LogError("SendGrid settings are missing. Check environment variables.");
            return;
        }

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, "Heartbeat Monitor");
        var subject = "Queue Heartbeat Triggered";
        var to = new EmailAddress(toEmail);
        var plainTextContent = $"New heartbeat received: {queueMessage}";
        var htmlContent = $"<strong>New heartbeat received: {queueMessage}</strong>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);

        _logger.LogInformation($"Email sent. Status Code: {response.StatusCode}");
    }
}