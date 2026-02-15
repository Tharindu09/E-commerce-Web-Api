using System.ComponentModel.DataAnnotations;

namespace PaymentService.Dtos;

public class GatewayPaymentRequest
{   [Required]
    public decimal Amount { get; set; }
    [Required]
    public string Currency { get; set; }
    [Required]
    public string PaymentMethodId { get; set; }
    [Required]
    public string IdempotencyKey { get; set; }
    [Required]
    public int OrderId { get; set; }
    [Required]
    public string Email { get; set; }
    
}

public class GatewayPaymentResult
{
    public bool Created { get; set; }           // PaymentIntent created successfully
    public bool RequiresAction { get; set; }    // 3D Secure / OTP required
    public string ClientSecret { get; set; }    // Only if RequiresAction = true
    public string PaymentIntentId { get; set; } // Stripe PI id
    public string ErrorMessage { get; set; }
}




