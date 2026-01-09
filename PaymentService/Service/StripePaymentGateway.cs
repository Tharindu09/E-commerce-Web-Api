using System;
using PaymentService.Dtos;
using Stripe;

namespace PaymentService.Service;

public class StripePaymentGateway : IPaymentGateway
{
    public async Task<GatewayPaymentResult> ChargeAsync(GatewayPaymentRequest request)
    {   
        try
        {
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
                    Enabled = true
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
            
            return new GatewayPaymentResult
            {
                Created = false,
                ErrorMessage = ex.StripeError?.Message ?? ex.Message
            };
        }
        
        

    }
}
