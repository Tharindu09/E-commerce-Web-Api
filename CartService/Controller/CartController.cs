using Microsoft.AspNetCore.Mvc;
using CartService.Model;
using CartService.Services;
using CartService.Dtos;
using System.Security.Claims;

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

    [HttpGet("my")]
    public async Task<IActionResult> GetCart()
    {    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Missing user id claim.");

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user id claim.");
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart ?? new Cart { UserId = userId });
    }


    [HttpPost("my/add")]
    public async Task<IActionResult> AddToCart(CartAddRequest req)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdStr))
            return Unauthorized("Missing user id claim.");

        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid user id claim.");

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
