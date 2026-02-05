using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Dtos;
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

        [Authorize]
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

        [Authorize]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateOrder(AddressDto addressDto)
        {

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Missing user id claim.");

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user id claim.");

            try
            {
                var orderId = await _orderService.CreateOrderAsync(userId,addressDto);
                return Ok(new { OrderId = orderId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Could not create order: {ex.Message}");
            }
        }
        
        [Authorize]
        [HttpGet("myorders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Missing user id claim.");

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user id claim.");

            try
            {
                var orders = await _orderService.GetMyOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest($"Could not retrieve orders: {ex.Message}");
            }
        }
    }
}