using CartService.Grpc;
using Grpc.Core;
using StackExchange.Redis;
using System.Text.Json;

public class CartGrpcService : CartService.Grpc.CartService.CartServiceBase
{
    private readonly IDatabase _redis;

    public CartGrpcService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public override async Task<CartResponse> GetCartByUserId(
        GetCartRequest request,
        ServerCallContext context)
    {
        var data = await _redis.StringGetAsync($"cart:{request.UserId}");

        if (data.IsNullOrEmpty)
        {
            return new CartResponse
            {
                UserId = request.UserId
            };
        }

        var cart = JsonSerializer.Deserialize<CartService.Model.Cart>(data!);

        var response = new CartResponse
        {
            UserId = cart!.UserId
        };

        foreach (var item in cart.Items)
        {
            response.Items.Add(new CartItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = (double)item.Price,
                Quantity = item.Quantity
            });
        }

        return response;
    }
}
