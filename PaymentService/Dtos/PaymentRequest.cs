namespace PaymentService.Dtos;

public class PaymentRequest
{
    public int OrderId { get; set; }
    public string PaymentType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string IdempotencyKey { get; set; }

}
