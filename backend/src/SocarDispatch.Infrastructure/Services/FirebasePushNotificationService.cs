using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using SocarDispatch.Application.Common.Interfaces;

namespace SocarDispatch.Infrastructure.Services;

public class FirebasePushNotificationService : IPushNotificationService
{
    private readonly ILogger<FirebasePushNotificationService> _logger;

    public FirebasePushNotificationService(ILogger<FirebasePushNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(deviceToken)) return;

        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data ?? new Dictionary<string, string>(),
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    Sound = "default"
                }
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message, ct);
            _logger.LogInformation("FCM Single Push Notification successfully sent. Message ID: {MessageId}", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send FCM single push notification to token {Token}", deviceToken);
        }
    }

    public async Task SendMulticastAsync(
        IEnumerable<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        var tokens = deviceTokens.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
        if (!tokens.Any())
        {
            _logger.LogDebug("No target device tokens provided for FCM multicast message.");
            return;
        }

        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data ?? new Dictionary<string, string>(),
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    Sound = "default"
                }
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, ct);
            _logger.LogInformation("FCM Multicast sent: {SuccessCount} success, {FailureCount} failure out of {TotalCount} tokens.",
                response.SuccessCount, response.FailureCount, tokens.Count);

            if (response.FailureCount > 0)
            {
                foreach (var item in response.Responses.Select((r, idx) => new { Response = r, Token = tokens[idx] }))
                {
                    if (!item.Response.IsSuccess)
                    {
                        _logger.LogWarning("FCM token failed delivery: {Token}. Error: {Error}", item.Token, item.Response.Exception?.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Push notification errors should not affect the DB transaction or the main flow.
            _logger.LogError(ex, "FCM multicast push notification process failed unexpectedly.");
        }
    }
}
