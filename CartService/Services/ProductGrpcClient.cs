
using ProductService.Grpc;

namespace CartService.Services;

public class ProductGrpcClient
{
    private readonly ProductService.Grpc.ProductService.ProductServiceClient _client;

    public ProductGrpcClient(ProductService.Grpc.ProductService.ProductServiceClient client)
    {
        _client = client;
    }

    public async Task<GetProductResponse?> GetProductByIdAsync(int productId)
    {
        try
        {
            var request = new GetProductReq
            {
                ProductId = productId
            };

            return await _client.GetProductByIdAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

}
