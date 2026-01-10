using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace PaymentService.Controller
{


    [ApiController]
    [Route("api/payments/webhook")]
    public class PaymentsWebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly Service.PaymentService _service;

        private readonly ILogger<PaymentsWebhookController> _logger;

        public PaymentsWebhookController(
            Service.PaymentService service,
            IConfiguration config,
            ILogger<PaymentsWebhookController> logger)
        {   
            _service = service;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    _config["Stripe:WebhookSecret"]
                );
            }
            catch (Exception)
            {
                _logger.LogError("Failed to parse webhook");
                return Unauthorized();
            }
            _logger.LogInformation("Received Stripe webhook: {EventType}", stripeEvent.Type);
            switch (stripeEvent.Type)
            {   
                case "payment_intent.succeeded":
                    await _service.HandlePaymentSucceeded(stripeEvent);
                    break;

                case "payment_intent.payment_failed":
                    await _service.HandlePaymentFailed(stripeEvent);
                    break;
            }

            return Ok();
        }

        
    }
}
