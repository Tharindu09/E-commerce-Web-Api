using OrderService.Dtos;
using OrderService.Model;
using PaymentService.Dtos;

namespace OrderService.Services;

public interface IOrderService
{
    Task<int> CreateOrderAsync(int userId, AddressDto addressDto);
    Task<OrderDto> GetOrderByIdAsync(int orderId);

    Task<Order> UpdateOrderAsync(PaymentKafkaDto paymentInfo);

    Task<List<OrderDto>> GetMyOrdersAsync(int userId);
}
