using Microsoft.AspNetCore.Mvc;
using OrderService.Dtos;
using OrderService.Model;
using OrderService.Services;

namespace OrderService.Controller
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Route("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                OrderDto order = await _orderService.GetOrderByIdAsync(orderId);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest($"Could not retrieve order: {ex.Message}");
            } 

        }

        [HttpPost]
        [Route("create/{userId}")]
        public async Task<IActionResult> CreateOrder(int userId)
        {
            try
            {
                var orderId = await _orderService.CreateOrderAsync(userId);
                return Ok(new { OrderId = orderId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Could not create order: {ex.Message}");
            }
        }
    }
}
