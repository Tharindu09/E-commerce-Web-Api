using System;

namespace NotificationService.Service;

public interface INotificationService
{
    Task SendPaymentNotificationAsync(string toEmail,
        string orderId,
        string status,
        decimal amount,
        string currency,
        CancellationToken ct = default);
}
