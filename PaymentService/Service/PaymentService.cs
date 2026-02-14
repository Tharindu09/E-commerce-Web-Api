
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaymentService.Data;
using PaymentService.Dtos;
using PaymentService.Models;
using Stripe;
using Stripe.V2;

namespace PaymentService.Service;

public class PaymentService
{
    private readonly OrderService.Grpc.OrderService.OrderServiceClient _orderClient;
    private readonly PaymentDbContext _db;
    private readonly KafkaProducerService _kafkaProducer;
    private ILogger<PaymentService> _logger;
    private readonly IPaymentGateway _paymentGateway;

    public PaymentService(OrderService.Grpc.OrderService.OrderServiceClient orderClient, PaymentDbContext db, IPaymentGateway paymentGateway, ILogger<PaymentService> logger, KafkaProducerService kafkaProducer)
    
    {
        _orderClient = orderClient;
        _paymentGateway = paymentGateway;
        _db = db;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(GatewayPaymentRequest request, int userId)
    {   
        // Check for existing payment with the same IdempotencyKey
        var existingPayment = await _db.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == request.IdempotencyKey);

        if (existingPayment != null)
        {
            return new PaymentResponse
            {
                PaymentId = existingPayment.PaymentId,
                PaymentStatus = existingPayment.Status
            };
        }
        try
        {
            // Fetch order details via gRPC
            var orderDetails = await GetOrderDetailsAsync(request.OrderId);
            // Validate order status
            if (orderDetails == null)
            {
                throw new Exception("Order details not found.");
            }
            if (orderDetails.OrderStatus != "PendingPayment")
            {
                throw new Exception("Order is not in a valid state for payment.");
            }
            if (orderDetails.TotalAmount != (double)request.Amount)
            {
                throw new Exception("Payment amount does not match order total.");
            }

        }
        catch (Exception ex)
        {
            
            throw;
        }
        
        

        var payment = new Payment
        {
            OrderId = request.OrderId,
            UserId = userId,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = PaymentStatus.PENDING.ToString(),
            IdempotencyKey = request.IdempotencyKey
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        // STRIPE payment processing logic
        var intent = await _paymentGateway.ChargeAsync(new GatewayPaymentRequest
        {
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentMethodId = request.PaymentMethodId,
            IdempotencyKey = request.IdempotencyKey
        });
        _logger.LogInformation("Payment intent created with status: {Status} for Payment ID: {PaymentId}", intent, payment.PaymentId);
        payment.Status = intent.Created ? PaymentStatus.PROCESSING.ToString() : PaymentStatus.FAILED.ToString();
        payment.GatewayPaymentIntentId = intent.PaymentIntentId;
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync();

        

        return new PaymentResponse
        {
            PaymentId = payment.PaymentId,
            PaymentStatus = payment.Status,
            ClientSecret = intent.ClientSecret
        };
    }

    public async Task<OrderService.Grpc.OrderResponse> GetOrderDetailsAsync(int orderId)
    {
        var request = new OrderService.Grpc.OrderRequest { OrderId = orderId };
        var response = await _orderClient.GetOrderByIdAsync(request);
        return response;
    }

    public async Task HandlePaymentSucceeded(Event stripeEvent)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        var idempotencyKey = intent.Metadata["IdempotencyKey"];
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);

        if (payment == null || payment.Status == PaymentStatus.PAID.ToString())
            return;

        payment.Status = PaymentStatus.PAID.ToString();
        payment.GatewayChargeId = intent.Id;

        _db.Payments.Update(payment);
        await _db.SaveChangesAsync();

        await _kafkaProducer.ProduceAsync(JsonConvert.SerializeObject(new
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status
        }));
        }

    public async Task HandlePaymentFailed(Event stripeEvent)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        var idempotencyKey = intent.Metadata["IdempotencyKey"]; 
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);

        if (payment == null || payment.Status == PaymentStatus.FAILED.ToString())
            return;

        payment.Status = PaymentStatus.FAILED.ToString();
        payment.GatewayChargeId = intent.Id;

        _db.Payments.Update(payment);
        await _db.SaveChangesAsync();

        await _kafkaProducer.ProduceAsync(JsonConvert.SerializeObject(new
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status
        }));
    }

    internal async Task<Payment> GetPaymentIntentAsync(int paymentId, int userId)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.UserId == userId);
        if (payment == null)
        {
            throw new Exception("Payment not found.");
        }
        if (payment.GatewayPaymentIntentId == null)
        {
            throw new Exception("Payment intent not created yet.");
        }

        return payment;
    }
}
