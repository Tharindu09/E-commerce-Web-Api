using CartService.Model;
using System.Text.Json;
using StackExchange.Redis;
using CartService.Dtos;

namespace CartService.Services;

public class CCartService : ICartService
{
    private readonly IDatabase _redis;
    private readonly ProductGrpcClient _client;

    public CCartService(IConnectionMultiplexer redis,ProductGrpcClient client)
    {
        _redis = redis.GetDatabase();
        _client = client;
    }
        public async Task<Cart?> GetCartAsync(int userId)
    {
        var data =await _redis.StringGetAsync($"cart:{userId}");
        if (data.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<Cart>(data!);

    }
    
    public async Task<Cart> AddToCartAsync(int userId, int productId, int quantity)
    {
    // Fetch product info from Product Service
    var product = await _client.GetProductByIdAsync(productId);
    if (product == null) throw new Exception("Product not found");

    var cart = await GetCartAsync(userId) ?? new Cart { UserId = userId };

    var existing = cart.Items.FirstOrDefault(x => x.ProductId == productId);
    if (existing != null)
    {
        existing.Quantity += quantity;
        existing.Price = (decimal)product.Price; 
        existing.ProductName = product.Name;
    }
    else
    {
        cart.Items.Add(new CartItem
        {
            ProductId = productId,
            ProductName = product.Name,
            Price = (decimal)product.Price,
            Quantity = quantity
        });
    }

    await _redis.StringSetAsync($"cart:{userId}", JsonSerializer.Serialize(cart), TimeSpan.FromDays(7));
    return cart;
}

    public async Task<bool> ClearCartAsync(int userId)
    {
        return await _redis.KeyDeleteAsync($"cart:{userId}");
    }

    
    public async Task<bool> RemoveItemAsync(int userId, int productId)
    {
        var cart = await GetCartAsync(userId);
        if (cart == null) return false;

        cart.Items.RemoveAll(x => x.ProductId == productId);
        await _redis.StringSetAsync($"cart:{userId}", JsonSerializer.Serialize(cart));
        return true;

    }

    
}
