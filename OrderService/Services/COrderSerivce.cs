using CartService.Grpc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Dtos;
using OrderService.Model;
using PaymentService.Dtos;
using ProductService.Grpc;
using UserService.Grpc;

namespace OrderService.Services;

public class COrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly CartService.Grpc.CartService.CartServiceClient _cartClient;
    private readonly UserProfileService.UserProfileServiceClient _userClient;
    private readonly ProductService.Grpc.ProductService.ProductServiceClient _productClient;
    private readonly ILogger<COrderService> _logger;
    public COrderService(
        AppDbContext db,
        CartService.Grpc.CartService.CartServiceClient cartClient,
        UserProfileService.UserProfileServiceClient userClient,
        ProductService.Grpc.ProductService.ProductServiceClient productClient,
        ILogger<COrderService> logger)
    {
        _db = db;
        _cartClient = cartClient;
        _userClient = userClient;
        _productClient = productClient;
        _logger = logger;
    }

    public async Task<int> CreateOrderAsync(int userId)
    {
        // Fetch cart
        var cart = await GetCartAsync(userId);

        // Fetch user profile
        var user = await GetUserAsync(userId);

        // Start transaction
        using var tx = await _db.Database.BeginTransactionAsync();

        Order order = new Order();

        try
        {
            order = await CreateDraftOrderAsync(user, cart);
            await ReserveStockAsync(order.Id, cart);
            await FinalizeOrderAsync(order);

            await tx.CommitAsync();
            return order.Id;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Order creation failed for User {UserId}", userId);
            await CancelStockReservationAsync(order.Id);
            throw;
        }
    }

    public async Task<OrderDto> GetOrderByIdAsync(int orderId)
    {
        var order = await _db.Orders
            .Where(o => o.Id == orderId)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserName = o.UserName,
                UserEmail = o.UserEmail,
                UserPhone = o.UserPhone,
                ShipLine1 = o.ShipLine1,
                ShipLine2 = o.ShipLine2,
                ShipCity = o.ShipCity,
                ShipDistrict = o.ShipDistrict,
                ShipProvince = o.ShipProvince,
                ShipPostalCode = o.ShipPostalCode,
                OrderStatus = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    PriceAtPurchase = i.PriceAtPurchase
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
            throw new Exception("Order not found");


        return order;
    }


    // Helper methods
    private async Task<CartService.Grpc.CartResponse> GetCartAsync(int userId)
    {
        var cart = await _cartClient.GetCartByUserIdAsync(
            new GetCartRequest { UserId = userId });

        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty");
        }

        return cart;
    }

    private async Task<UserProfileResponse> GetUserAsync(int userId)
    {
        return await _userClient.GetUserProfileAsync(
            new GetUserProfileRequest { UserId = userId });
    }

    private async Task<Order> CreateDraftOrderAsync(
    UserProfileResponse user,
    CartService.Grpc.CartResponse cart)
    {
        var order = new Order
        {
            UserId = user.UserId,
            UserName = user.Name,
            UserEmail = user.Email,
            UserPhone = user.Phone,
            ShipLine1 = user.AddressLine1,
            ShipLine2 = user.AddressLine2,
            ShipCity = user.City,
            ShipDistrict = user.District,
            ShipProvince = user.Province,
            ShipPostalCode = user.PostalCode,
            OrderStatus = OrderStatus.Draft.ToString(),
            TotalAmount = cart.Items.Sum(i => (decimal)i.Price * i.Quantity),
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                PriceAtPurchase = (decimal)i.Price,
                Quantity = i.Quantity
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return order;
    }

    private async Task ReserveStockAsync(int orderId, CartService.Grpc.CartResponse cart)
    {
        var request = new ReserveStockRequest
        {
            OrderId = orderId
        };
        request.Items.AddRange(
            cart.Items.Select(i => new Stock
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }));

        var response = await _productClient.ReserveStockAsync(request);

        if (!response.Success)
        {
            throw new InvalidOperationException(
                $"Stock reservation failed: {response.Message}");
        }
    }

    private async Task FinalizeOrderAsync(Order order)
    {
        order.OrderStatus = OrderStatus.PendingPayment.ToString();
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Order {OrderId} moved to PendingPayment",
            order.Id);
    }

    private async Task CancelStockReservationAsync(int orderId)
    {
        await _productClient.CancelReservationAsync(
            new CancelReservationRequest { OrderId = orderId });
    }

    public async Task<Order> UpdateOrderAsync(PaymentKafkaDto paymentInfo)
    {
        var status = paymentInfo.Status.ToLower();
        var order = _db.Orders.Include(o => o.Items)
            .FirstOrDefault(o => o.Id == paymentInfo.OrderId);
        if (order == null)
        {
            throw new Exception("Order not found.");
        }
        if (status == "paid")
        {
            order.OrderStatus = OrderStatus.Paid.ToString();

        }
        else if (status == "failed")
        {
            order.OrderStatus = OrderStatus.PendingPayment.ToString();

        }
        else
        {
            _logger.LogWarning("Unknown payment status: {Status}", paymentInfo.Status);
            throw new Exception("Unknown payment status.");
        }
        await _db.SaveChangesAsync();
        return order;
    }
}
