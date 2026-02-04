using Microsoft.AspNetCore.Mvc;
using CartService.Model;
using CartService.Services;
using CartService.Dtos;

namespace CartService.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }
    
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCart(int userId)
    {
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart ?? new Cart { UserId = userId });
    }


    [HttpPost("{userId}/add")]
    public async Task<IActionResult> AddToCart(int userId, CartAddRequest req)
    {
        try
        {
            var cart = await _cartService.AddToCartAsync(userId, req.ProductId, req.Quantity);
            return Ok(cart);
        }
        catch (Exception ex)
        {

            return BadRequest($"Could not add item to cart: {ex.Message}");
        }
    }

    [HttpDelete("{userId}/remove/{productId}")]
    public async Task<IActionResult> RemoveItem(int userId, int productId)
    {
        var result = await _cartService.RemoveItemAsync(userId, productId);
        return Ok(result);
    }

    [HttpDelete("{userId}/clear")]
    public async Task<IActionResult> ClearCart(int userId)
    {
        var result = await _cartService.ClearCartAsync(userId);
        return Ok(result);
    }
}
