using System;
using PaymentService.Dtos;
using Stripe;

namespace PaymentService.Service;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly ILogger<StripePaymentGateway> _logger;
    public StripePaymentGateway(ILogger<StripePaymentGateway> logger)
    {
        _logger = logger;
    }
    
    public async Task<GatewayPaymentResult> ChargeAsync(GatewayPaymentRequest request)
    {   
        try
        {   _logger.LogInformation("Initiating charge with Stripe for Amount: {Amount}, Currency: {Currency}", request.Amount, request.Currency);
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100), // Stripe expects amount in cents
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethodId,
                Confirm = true,
                Metadata = new Dictionary<string, string>
                {
                    { "IdempotencyKey", request.IdempotencyKey }
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"

                }

            };

            var service = new PaymentIntentService();

            var paymentIntent = await service.CreateAsync(options, new RequestOptions
            {
                IdempotencyKey = request.IdempotencyKey
            });

            return new GatewayPaymentResult
            {
                Created = paymentIntent.Status == "succeeded",
                RequiresAction = paymentIntent.Status == "requires_action",
                ClientSecret = paymentIntent.Status == "requires_action" ? paymentIntent.ClientSecret : null,
                PaymentIntentId = paymentIntent.Id,
            };




        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error occurred: {Message}", ex.Message);
            return new GatewayPaymentResult
            {
                Created = false,
                ErrorMessage = ex.StripeError?.Message ?? ex.Message
            };
        }
        
        

    }
}
