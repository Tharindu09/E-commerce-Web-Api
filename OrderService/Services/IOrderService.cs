using OrderService.Dtos;
using OrderService.Model;

namespace OrderService.Services;

public interface IOrderService
{
    Task<int> CreateOrderAsync(int userId);
    Task<OrderDto> GetOrderByIdAsync(int orderId);
}
