using CartService.Grpc;
using OrderService.Data;
using OrderService.Model;
using UserService.Grpc;

namespace OrderService.Services;

public class COrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly CartService.Grpc.CartService.CartServiceClient _cartClient;
    private readonly UserProfileService.UserProfileServiceClient _userClient;
    private readonly IPaymentService _paymentService;

    public COrderService(
        AppDbContext db,
        CartService.Grpc.CartService.CartServiceClient cartClient,
        UserProfileService.UserProfileServiceClient userClient,
        IPaymentService paymentService)
    {
        _db = db;
        _cartClient = cartClient;
        _userClient = userClient;
        _paymentService = paymentService;
    }

    public async Task<int> CreateOrderAsync(int userId)
    {
        // Fetch cart
        var cart = await _cartClient.GetCartByUserIdAsync(
            new GetCartRequest { UserId = userId });

        if (cart.Items.Count == 0)
            throw new Exception("Cart is empty");

        // Fetch user profile
        var user = await _userClient.GetUserProfileAsync(
            new GetUserProfileRequest { UserId = userId });
        
        //Reserve stock
        

        // Begin transaction
        using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            // Create Order snapshot
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

                OrderStatus = OrderStatus.Created.ToString(),
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

            // Calculate total
            var totalAmount = order.Items.Sum(i => i.Total);

            // Payment (placeholder)
            var paymentOk = await _paymentService
                .InitiatePaymentAsync(order.Id, totalAmount);

            order.OrderStatus = paymentOk
                ? OrderStatus.Paid.ToString()
                : OrderStatus.PendingPayment.ToString();

            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return order.Id;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
