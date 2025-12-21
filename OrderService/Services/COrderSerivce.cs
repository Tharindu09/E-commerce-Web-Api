using CartService.Grpc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Dtos;
using OrderService.Model;
using ProductService.Grpc;
using UserService.Grpc;

namespace OrderService.Services;

public class COrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly CartService.Grpc.CartService.CartServiceClient _cartClient;
    private readonly UserProfileService.UserProfileServiceClient _userClient;
    private readonly IPaymentService _paymentService;
    private readonly ProductService.Grpc.ProductService.ProductServiceClient _productClient;
    private readonly ILogger<COrderService> _logger;
    public COrderService(
        AppDbContext db,
        CartService.Grpc.CartService.CartServiceClient cartClient,
        UserProfileService.UserProfileServiceClient userClient,
        IPaymentService paymentService,
        ProductService.Grpc.ProductService.ProductServiceClient productClient,
        ILogger<COrderService> logger)
    {
        _db = db;
        _cartClient = cartClient;
        _userClient = userClient;
        _paymentService = paymentService;
        _productClient = productClient;
        _logger = logger;
    }

    public async Task<int> CreateOrderAsync(int userId)
    {
        // Fetch cart
        var cart = await _cartClient.GetCartByUserIdAsync(
            new GetCartRequest { UserId = userId });

        if (cart.Items.Count == 0)
        {
            _logger.LogWarning("Attempt to create order with empty cart for User ID: {UserId}", userId);
            throw new Exception("Cart is empty");

        }

        // Fetch user profile
        var user = await _userClient.GetUserProfileAsync(
            new GetUserProfileRequest { UserId = userId });

        //Save Order draft
        var orderDraft = new Order
        {
            UserId = userId,
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
            Items = new List<OrderItem>()
        };
        await _db.Orders.AddAsync(orderDraft);
        await _db.SaveChangesAsync();

        //Reserve stock  
        try
        {
            ReserveStockRequest request = new ReserveStockRequest
            {
                OrderId = orderDraft.Id,
            };
            foreach (var item in cart.Items)
            {
                request.Items.Add(new Stock
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            var reservationResponse = await _productClient.ReserveStockAsync(request);
            
        }catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stock reservation for Order ID: {OrderId}", orderDraft.Id);
           
            throw;
        }
        


        // Begin transaction
        _logger.LogInformation("Beginning transaction for creating order for User ID: {UserId}", userId);
        int orderId;

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                UserId = userId,
                UserName = user.Name,
                UserEmail = user.Email,
                UserPhone = user.Phone,

                ShipLine1 = user.AddressLine1,
                ShipLine2 = user.AddressLine2,
                ShipCity = user.City,
                ShipDistrict = user.District,
                ShipProvince = user.Province,
                ShipPostalCode = user.PostalCode,

                OrderStatus = OrderStatus.PendingPayment.ToString(),
                Items = new List<OrderItem>()
            };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    PriceAtPurchase = (decimal)item.Price,
                    Quantity = item.Quantity
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            orderId = order.Id;
        }
        catch
        {
            await tx.RollbackAsync();
            _logger.LogError("Transaction rolled back for creating order for User ID: {UserId}", userId);
            await _productClient.CancelReservationAsync(new CancelReservationRequest
            {
                OrderId = orderDraft.Id
            });
            throw;
        }

        return orderId;

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

}
