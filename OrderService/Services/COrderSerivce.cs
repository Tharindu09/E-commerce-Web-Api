using CartService.Grpc;
using OrderService.Data;
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

        //Reserve stock
        var reservedItems = new List<CartItem>();
        try
        {
            foreach (var item in cart.Items)
            {
                UpdateInventoryResponse response = await _productClient.UpdateInventoryAsync(
                    new UpdateInventoryRequest
                    {
                        ProductId = item.ProductId,
                        QuantityChange = item.Quantity * -1, // Reduce stock
                    });
                if (!response.Success)
                {
                    _logger.LogError("Failed to reserve stock for Product ID: {ProductId}", item.ProductId);
                    throw new Exception($"Failed to reserve stock for Product ID: {item.ProductId}");
                }
                reservedItems.Add(item);

            }
        }
        catch
        {

            // ROLLBACK INVENTORY
            foreach (var item in reservedItems)
            {
                await _productClient.UpdateInventoryAsync(
                    new UpdateInventoryRequest
                    {
                        ProductId = item.ProductId,
                        QuantityChange = item.Quantity // restore
                    });
            }

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
            throw;
        }
        return 1;

    }

    
}
