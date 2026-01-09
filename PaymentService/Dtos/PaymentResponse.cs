using System;

namespace PaymentService.Dtos;

public class PaymentResponse
{
    public int PaymentId { get; set; }
    public string PaymentStatus { get; set; }

    public string ClientSecret { get; set; } // optional
}
