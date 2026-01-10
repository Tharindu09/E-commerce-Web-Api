
namespace PaymentService.Dtos;

public class PaymentKafkaDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
}
