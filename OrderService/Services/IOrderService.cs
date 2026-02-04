using OrderService.Dtos;
using OrderService.Model;
using PaymentService.Dtos;

namespace OrderService.Services;

public interface IOrderService
{
    Task<int> CreateOrderAsync(int userId);
    Task<OrderDto> GetOrderByIdAsync(int orderId);

    Task<Order> UpdateOrderAsync(PaymentKafkaDto paymentInfo);

    Task<OrderDto> GetMyOrdersAsync(int userId);
}
