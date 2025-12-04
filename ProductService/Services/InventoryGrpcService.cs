using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Grpc;
using ProductService.Model;

namespace ProductService.Services;

public class InventoryGrpcService: InventoryGrpc.InventoryGrpcBase
{
    private readonly AppDbContext _context;

    public InventoryGrpcService(AppDbContext context)
    {
        _context = context;
    }

    public async override Task<UpdateInventoryResponse> UpdateInventory(UpdateInventoryRequest request, ServerCallContext context)
    {
        Product product = await _context.Products.Include(p => p.Inventory).SingleOrDefaultAsync(p => p.Id == request.ProductId);

        if (product == null)
        {
            return new UpdateInventoryResponse
            {
                Success = false,
                Message = "Product not found",
                NewStock = 0
            };
        }

        product.Inventory.Stock += request.QuantityChange;

        if (product.Inventory.Stock < 0)
        {
            return new UpdateInventoryResponse
            {
                Success = false,
                Message = "Not enough quantity",
                NewStock = 0
            };
        }

        await _context.SaveChangesAsync();

        return new UpdateInventoryResponse
        {
            Success = true,
            Message = "Inventory updated",
            NewStock = product.Inventory.Stock
        };


    }


}
