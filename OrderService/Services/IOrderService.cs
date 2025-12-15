namespace OrderService.Services;

public interface IOrderService
{
    Task<int> CreateOrderAsync(int userId);
}
