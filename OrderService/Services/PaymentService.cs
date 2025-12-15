namespace OrderService.Services;

public class PaymentService : IPaymentService
{
    public Task<bool> InitiatePaymentAsync(int orderId, decimal amount)
    {
        // Placeholder for real gateway (Stripe, PayHere, etc.)
        return Task.FromResult(true);
    }
}