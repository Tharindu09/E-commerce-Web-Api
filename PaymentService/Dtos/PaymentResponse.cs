using System;

namespace PaymentService.Dtos;

public class PaymentResponse
{
    public int PaymentId { get; set; }
    public string PaymentStatus { get; set; }
    public int OrderId { get; set; }
    public string Amount { get; set; }
    public string Currency { get; set; }
    public DateTime CreatedAt { get; set; } 

    public string ClientSecret { get; set; } // For 3D Secure flows

}