using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Grpc;
using ProductService.Model;

namespace ProductService.Services;

public class ProductGrpcService : Grpc.ProductService.ProductServiceBase
{
    private readonly AppDbContext _context;

    public ProductGrpcService(AppDbContext context)
    {
        _context = context;
    }

    // UPDATE INVENTORY Total Stock(ADMIN USE ONLY)
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

        product.Inventory.TotalStock += request.QuantityChange;

        if (product.Inventory.TotalStock < 0)
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
            NewStock = product.Inventory.TotalStock
        };


    }

    public async override Task<GetProductResponse> GetProductById(GetProductReq request, ServerCallContext context)
    {
        Product product = await _context.Products.Include(p => p.Inventory).SingleOrDefaultAsync(p => p.Id == request.ProductId);

        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
        }

        var response = new GetProductResponse
        {
            Name = product.Name,
            Price = (double)product.Price,
            Category = product.Category,
            Stock = product.Inventory.AvailableStock
        };

        return response;
    }

    // RESERVE STOCK
    public override async Task<ReserveStockResponse> ReserveStock(
    ReserveStockRequest request, ServerCallContext context)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        var product = await _context.Products.Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product == null)
            return Fail("Product not found");

        if (product.Inventory.AvailableStock < request.Quantity)
            return Fail("Insufficient stock");

        // Reduce available stock
        product.Inventory.AvailableStock -= request.Quantity;

        var reservation = new StockReservation
        {
            InventoryId = product.Inventory.Id,
            ProductId = product.Id,
            OrderId = request.OrderId,
            ReservedQuantity = request.Quantity,
            Status = ReservationStatus.Reserved,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10) //TTL 10 minutes
        };

        _context.StockReservations.Add(reservation);
        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return new ReserveStockResponse
        {
            Success = true,
            ReservationId = reservation.Id,
            Message = "Stock reserved"
        };
    }

    public override async Task<ConfirmReservationResponse> ConfirmReservation(
    ConfirmReservationRequest request, ServerCallContext context)
    {
        var reservation = await _context.StockReservations
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId);

        if (reservation == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Reservation not found"));

        if (reservation.Status != ReservationStatus.Reserved)
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Reservation cannot be confirmed"));

        reservation.Status = ReservationStatus.Confirmed;
        await _context.SaveChangesAsync();
        return new ConfirmReservationResponse
        {
            Success = true
        };
    }

    public override async Task<CancelReservationResponse> CancelReservation(
    CancelReservationRequest request, ServerCallContext context)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        var reservation = await _context.StockReservations
            .Include(r => r.Inventory)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId);

        if (reservation == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Reservation not found"));

        if (reservation.Status != ReservationStatus.Reserved)
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Reservation cannot be cancelled"));

        // Restore stock
        reservation.Inventory.AvailableStock += reservation.ReservedQuantity;

        reservation.Status = ReservationStatus.Cancelled;
        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return new CancelReservationResponse
        {
            Success = true
        };
    }

    // Helper method to create failure response
    private ReserveStockResponse Fail(string message)
    {
        return new ReserveStockResponse
        {
            Success = false,
            Message = message,
            ReservationId = 0
        };


    }
}
