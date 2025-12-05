using CartService.Model;
using System.Text.Json;
using StackExchange.Redis;

namespace CartService.Services;

public class CCartService : ICartService
{
    private readonly IDatabase _redis;

    public CCartService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task<Cart?> GetCartAsync(int userId)
    {
        var data =await _redis.StringGetAsync($"cart:{userId}");
        if (data.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<Cart>(data!);

    }
    
    public async Task<Cart> AddToCartAsync(int userId, CartItem item)
    {
        var cart = await GetCartAsync(userId) ?? new Cart { UserId = userId };
        var exist = cart.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (exist != null)
        {
            exist.Quantity += item.Quantity;
        }

        else cart.Items.Add(item);

        await _redis.StringSetAsync($"cart:{userId}", JsonSerializer.Serialize(cart)); //save cart

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
