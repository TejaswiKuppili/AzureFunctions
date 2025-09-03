using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;

namespace HeartBeatFunctionApp;

public class TimerHeartbeat
{
    private readonly ILogger _logger;
    private static DateTime? _lastHeartbeat;

    public TimerHeartbeat(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TimerHeartbeat>();
    }

    /// <summary>
    /// "0 */1 * * * *" = run every 1 minute at 0 seconds.
    /// For faster testing, use "*/30 * * * * *" (every 30 seconds).
    /// </summary>
    /// <param name="myTimer"></param>
    [Function("TimerHeartbeat")]
    public async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer)
    {
        var timestamp = DateTime.UtcNow.ToString("u");
        _logger.LogInformation("Timer heartbeat recorded at: {time}", timestamp);

        // 🔹 Pull values from environment (local.settings.json or Azure Configuration)
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var fromEmail = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL");
        var toEmail = Environment.GetEnvironmentVariable("SENDGRID_TO_EMAIL");

        var client = new SendGridClient(apiKey);

        var from = new EmailAddress(fromEmail, "Heartbeat Monitor");
        var to = new EmailAddress(toEmail);

        var subject = "Timer Heartbeat Triggered";
        var plainTextContent = $"Heartbeat triggered at {timestamp}";
        var htmlContent = $"<strong>Heartbeat triggered at {timestamp}</strong>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);

        _logger.LogInformation("Email sent with status code: {statusCode}", response.StatusCode);
    }
}