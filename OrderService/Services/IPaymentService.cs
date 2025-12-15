namespace OrderService.Services;

public interface IPaymentService
{
    Task<bool> InitiatePaymentAsync(int orderId, decimal amount);
}
