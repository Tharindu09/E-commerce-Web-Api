using System.ComponentModel.DataAnnotations;

namespace PaymentService.Dtos;

public class PaymentRequest
{   [Required]
    public int OrderId { get; set; }

    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string Currency { get; set; }
    
    [Required]
    public string IdempotencyKey { get; set; }
    
    [Required]
    public string PaymentMethodId { get; set; }

}
