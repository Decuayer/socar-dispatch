namespace SocarDispatch.Application.Common.Interfaces;

public interface IPushNotificationService
{
    Task SendAsync(string deviceToken, string title, string body,
                   Dictionary<string, string>? data = null, CancellationToken ct = default);

    Task SendMulticastAsync(IEnumerable<string> deviceTokens, string title, string body,
                            Dictionary<string, string>? data = null, CancellationToken ct = default);
}
