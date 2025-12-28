using System;

namespace PaymentService.Models
{
    public class Payment
    {
        public int PaymentId { get; set; } 
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentType { get; set; } // e.g., CreditCard, PayPal
        public string Status { get; set; } // PENDING, PAID, FAILED
        public string IdempotencyKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string GatewayResponse { get; set; }="";
    }
}

enum PaymentStatus
{
    PENDING,
    PAID,
    FAILED
}
