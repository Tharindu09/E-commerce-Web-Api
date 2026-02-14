using System;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models
{
    public class Payment
    {   [Key]
        public int PaymentId { get; set; } 
        [Required]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Gateway { get; set; }  = "STRIPE";
        public string Status { get; set; } 
        public string IdempotencyKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? GatewayPaymentIntentId { get; set; }

        public string? GatewayChargeId { get; set; }
    }
}

enum PaymentStatus
{
    PENDING,
    PROCESSING,
    PAID,
    FAILED
}
