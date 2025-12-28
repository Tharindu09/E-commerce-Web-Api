
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaymentService.Data;
using PaymentService.Dtos;
using PaymentService.Models;

namespace PaymentService.Service;

public class PaymentService
{
    private readonly OrderService.Grpc.OrderService.OrderServiceClient _orderClient;
    private readonly PaymentDbContext _db;
    private readonly KafkaProducerService _kafkaProducer;
    private const string Topic = "payment-events";

    public PaymentService(OrderService.Grpc.OrderService.OrderServiceClient orderClient, PaymentDbContext db, KafkaProducerService kafkaProducer)
    {
        _orderClient = orderClient;
        _db = db;
        _kafkaProducer = kafkaProducer;
    }
    
    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
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
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentType = request.PaymentType,
            Status = PaymentStatus.PENDING.ToString(),
            IdempotencyKey = request.IdempotencyKey
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        // Simulate payment processing logic
        bool paymentSuccess = SimulateGatewayPayment();

        payment.Status = paymentSuccess ? PaymentStatus.PAID.ToString() : PaymentStatus.FAILED.ToString();
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync();

        // Publish payment event to Kafka
        await _kafkaProducer.ProduceAsync(JsonConvert.SerializeObject(new
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            Status = payment.Status
        }));


        return new PaymentResponse
        {
            PaymentId = payment.PaymentId,
            PaymentStatus = payment.Status
        };
    }

    public async Task<OrderService.Grpc.OrderResponse> GetOrderDetailsAsync(int orderId)
    {
        var request = new OrderService.Grpc.OrderRequest { OrderId = orderId };
        var response = await _orderClient.GetOrderByIdAsync(request);
        return response;
    }

    private bool SimulateGatewayPayment()
    {
        // Fake random payment success/failure
        return true;
    }
        


}
