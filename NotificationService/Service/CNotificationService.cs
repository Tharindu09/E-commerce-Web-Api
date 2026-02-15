using System;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace NotificationService.Service;

public sealed class SmtpOptions
{
    public string Host { get; init; } = "";
    public int Port { get; init; } = 587;
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
    public string FromEmail { get; init; } = "";
    public string FromName { get; init; } = "Notifications";
    public bool UseStartTls { get; init; } = true;
}

public class SmtpEmailSender  : INotificationService
{
    private readonly SmtpOptions _opt;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _opt = options.Value;
    }

   public async Task SendPaymentNotificationAsync(
    string toEmail,
    string orderId,
    string status,
    decimal amount,
    string currency,
    CancellationToken ct = default)
{
    var isSuccess = status.Equals("PAID", StringComparison.OrdinalIgnoreCase);

    var subject = isSuccess
        ? $"Payment confirmed for Order {orderId}"
        : $"Payment failed for Order {orderId}";

    var statusText = isSuccess ? "Successful" : "Failed";
    var accentColor = isSuccess ? "#16a34a" : "#dc2626"; // green or red
    var badgeBg = isSuccess ? "#dcfce7" : "#fee2e2";
    var badgeText = isSuccess ? "#166534" : "#991b1b";

    var plainText =
$@"Hello,

Your payment status has been updated.

Order ID: {orderId}
Status: {statusText}
Amount: {amount:0.00} {currency}

If you have questions, contact support.

Thank you.";

    var html =
$@"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <title>{subject}</title>
</head>
<body style=""margin:0; padding:0; background:#f6f7fb; font-family: Arial, Helvetica, sans-serif; color:#0f172a;"">
  <div style=""max-width:640px; margin:0 auto; padding:24px;"">

    <div style=""padding:18px 20px; background:#ffffff; border-radius:14px; border:1px solid #e5e7eb;"">
      <div style=""display:flex; align-items:center; gap:10px;"">
        <div style=""width:10px; height:10px; border-radius:999px; background:{accentColor};""></div>
        <div style=""font-size:18px; font-weight:700;"">
          { (isSuccess ? "Payment Confirmed" : "Payment Unsuccessful") }
        </div>
      </div>

      <p style=""margin:12px 0 0; color:#334155; line-height:1.6;"">
        Hello,<br>
        { (isSuccess
            ? "We have received your payment successfully. Below is your receipt summary."
            : "Your payment could not be completed. Below is the attempt summary.") }
      </p>

      <div style=""margin-top:18px; padding:16px; border-radius:12px; background:#f8fafc; border:1px solid #e2e8f0;"">
        <div style=""display:flex; justify-content:space-between; align-items:center; gap:12px;"">
          <div style=""font-size:14px; color:#64748b;"">Order Id &nbsp;</div>
          <div style=""font-size:14px; font-weight:700; color:#0f172a;"">{orderId}</div>
        </div>

        <div style=""height:10px;""></div>

        <div style=""display:flex; justify-content:space-between; align-items:center; gap:12px;"">
          <div style=""font-size:14px; color:#64748b;"">Status&nbsp;</div>
          <div style=""font-size:13px; font-weight:700; padding:6px 10px; border-radius:999px; background:{badgeBg}; color:{badgeText}; border:1px solid #e5e7eb;"">
            {statusText}
          </div>
        </div>

        <div style=""height:10px;""></div>

        <div style=""display:flex; justify-content:space-between; align-items:center; gap:12px;"">
          <div style=""font-size:14px; color:#64748b;"">Amount&nbsp;</div>
          <div style=""font-size:16px; font-weight:800; color:#0f172a;"">
            {amount:0.00} {currency}
          </div>
        </div>

        <div style=""margin-top:14px; border-top:1px dashed #cbd5e1;""></div>

        <div style=""margin-top:12px; display:flex; justify-content:space-between; align-items:center; gap:12px;"">
          <div style=""font-size:12px; color:#94a3b8;"">Receipt&nbsp;</div>
          <div style=""font-size:12px; color:#94a3b8;"">
            {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm} UTC
          </div>
        </div>
      </div>

      <div style=""margin-top:18px; padding:14px; border-radius:12px; border:1px solid #e5e7eb; background:#ffffff;"">
        <div style=""font-size:13px; font-weight:700; color:#0f172a;"">What to do next</div>
        <div style=""margin-top:8px; font-size:13px; color:#475569; line-height:1.6;"">
          {(isSuccess
            ? "No further action is required. You can keep this email as your payment receipt."
            : "Please try again using a different card or payment method. If you were charged, contact support and include the Order ID above.")}
        </div>

        {(isSuccess ? "" : $@"
        <div style=""margin-top:14px;"">
          <a href=""http://localhost:5173/payments/{orderId}"" style=""display:inline-block; text-decoration:none; background:{accentColor}; color:#ffffff; padding:10px 14px; border-radius:10px; font-weight:700; font-size:13px;"">
            Try payment again
          </a>
        </div>")}
      </div>

      <p style=""margin:18px 0 0; font-size:12px; color:#94a3b8; line-height:1.6;"">
        If you did not initiate this payment, please contact support immediately.
      </p>
    </div>

    <div style=""margin-top:14px; text-align:center; font-size:12px; color:#94a3b8;"">
      © {DateTime.UtcNow.Year} MAPA Ecom. All rights reserved.
    </div>

  </div>
</body>
</html>";

    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(_opt.FromName, _opt.FromEmail));
    message.To.Add(MailboxAddress.Parse(toEmail));
    message.Subject = subject;

    var bodyBuilder = new BodyBuilder
    {
        TextBody = plainText,
        HtmlBody = html
    };

    message.Body = bodyBuilder.ToMessageBody();

    using var client = new SmtpClient();

    var secure = _opt.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

    await client.ConnectAsync(_opt.Host, _opt.Port, secure, ct);

    if (!string.IsNullOrWhiteSpace(_opt.Username))
        await client.AuthenticateAsync(_opt.Username, _opt.Password, ct);

    await client.SendAsync(message, ct);
    await client.DisconnectAsync(true, ct);
}

}
