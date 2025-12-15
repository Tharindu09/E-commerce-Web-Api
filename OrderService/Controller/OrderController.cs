using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Controller
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly COrderService _orderService;
        public OrderController(COrderService orderService)
        {
            _orderService = orderService;
        }

        // [HttpGet]
        // [Route("{orderId}")]
        // public IActionResult GetOrderById(int ororderId)
        // {

        // }

        [HttpPost]
        [Route("create/{userId}")]
        public IActionResult CreateOrder(int userId)
        {
            try
            {
                var orderId = _orderService.CreateOrderAsync(userId).Result;
                return Ok(new { OrderId = orderId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
