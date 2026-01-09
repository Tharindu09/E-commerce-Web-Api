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
        public async Task<IActionResult> Post([FromBody]GatewayPaymentRequest request)
        {
            try
            {
                var response = await _paymentService.ProcessPaymentAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
