using CartService.Model;
using System.Text.Json;
using StackExchange.Redis;
using CartService.Dtos;

namespace CartService.Services;

public class CCartService : ICartService
{
    private readonly IDatabase _redis;
    private readonly HttpClient _http;

    public CCartService(IConnectionMultiplexer redis,HttpClient http)
    {
        _redis = redis.GetDatabase();
        _http = http;
    }
    public async Task<ProductDto?> GetProductById(int productId)
    {
        // Fetch product info from Product Service
        var product = await _http.GetFromJsonAsync<ProductDto>($"http://localhost:5258/api/products/{productId}");
        return product;
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
    var product = await GetProductById(productId);
    if (product == null) throw new Exception("Product not found");

    var cart = await GetCartAsync(userId) ?? new Cart { UserId = userId };

    var existing = cart.Items.FirstOrDefault(x => x.ProductId == productId);
    if (existing != null)
    {
        existing.Quantity += quantity;
        existing.Price = product.Price; // always override with correct price
        existing.ProductName = product.Name;
    }
    else
    {
        cart.Items.Add(new CartItem
        {
            ProductId = productId,
            ProductName = product.Name,
            Price = product.Price,
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
