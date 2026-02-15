using System;

namespace NotificationService.Dtos;

public class PaymentKafkaDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public string Email { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }
}
