using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Dtos;

namespace PaymentService.Controller
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly Service.PaymentService _paymentService;
        public PaymentController(Service.PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("process")]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] PaymentRequest request)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");
                 var email =User.FindFirstValue(ClaimTypes.Email) ??
                                User.FindFirstValue("email") ??
                                User.FindFirstValue("preferred_username") ??
                                User.FindFirstValue("upn");
                if (string.IsNullOrWhiteSpace(userIdStr) || string.IsNullOrWhiteSpace(email))
                    return Unauthorized("Missing required claims.");

                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized("Invalid user id claim.");
                Console.WriteLine("USer email:",email);
                var response = await _paymentService.ProcessPaymentAsync(request, userId,email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentIntent(int paymentId)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

                if (string.IsNullOrWhiteSpace(userIdStr))
                    return Unauthorized("Missing user id claim.");

                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized("Invalid user id claim.");
                var payment = await _paymentService.GetPaymentIntentAsync(paymentId, userId);
                var paymentResponse = new PaymentResponse
                {
                    PaymentId = payment.PaymentId,
                    PaymentStatus = payment.Status,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount.ToString("F2"),
                    Currency = payment.Currency,
                    CreatedAt = payment.CreatedAt
                };
                return Ok(paymentResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
            }
    }
}
