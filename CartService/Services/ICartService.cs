using CartService.Model;

namespace CartService.Services;

public interface ICartService
{
    Task<Cart?> GetCartAsync(int userId);
    Task<Cart> AddToCartAsync(int userId, int productId, int quantity);

    Task<bool> RemoveItemAsync(int userId, int productId);
    
    Task<bool> ClearCartAsync(int userId);
    
}
