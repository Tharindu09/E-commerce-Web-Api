using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Grpc;
using ProductService.Model;

namespace ProductService.Services;

public class ProductGrpcService : Grpc.ProductService.ProductServiceBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductGrpcService> _logger;

    public ProductGrpcService(AppDbContext context, ILogger<ProductGrpcService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // UPDATE INVENTORY Total Stock(ADMIN USE ONLY)
    public async override Task<UpdateInventoryResponse> UpdateInventory(UpdateInventoryRequest request, ServerCallContext context)
    {
        Product product = await _context.Products.Include(p => p.Inventory).SingleOrDefaultAsync(p => p.Id == request.ProductId);

        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
        }

        product.Inventory.TotalStock += request.QuantityChange;
        product.Inventory.AvailableStock += request.QuantityChange;

        if (product.Inventory.TotalStock < 0)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Total stock cannot be negative"));
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
            Stock = product.Inventory.AvailableStock,
            ImageUrl = product.ImageUrl
        };

        return response;
    }

    // RESERVE STOCK
    public override async Task<ReserveStockResponse> ReserveStock(
    ReserveStockRequest request, ServerCallContext context)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        var response = new ReserveStockResponse
        {
            Success = true,
            Message = "Stock reserved successfully"
        };
            
        var reservations = new List<StockReservation>();

        try
        {
            foreach (var item in request.Items)
            {
                var product = await _context.Products
                    .Include(p => p.Inventory)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Product not found: {item.ProductId}"));
                }

                if (product.Inventory.AvailableStock < item.Quantity)
                {
                    throw new RpcException(new Status(StatusCode.FailedPrecondition, $"Insufficient stock for product: {item.ProductId}"));
                }

                // Reduce available stock
                product.Inventory.AvailableStock -= item.Quantity;

                var reservation = new StockReservation
                {
                    InventoryId = product.Inventory.Id,
                    ProductId = product.Id,
                    OrderId = request.OrderId,
                    ReservedQuantity = item.Quantity,
                    Status = ReservationStatus.Reserved,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10) // TTL 10 minutes
                };

                _context.StockReservations.Add(reservation);
                reservations.Add(reservation);
            
                response.Results.Add(new StockReservationResult
                {
                    ProductId = item.ProductId,
                    ReservationId = reservation.Id,
                    Reserved = true
                });
            }

            // Save all reservations at once
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            _logger.LogInformation("Stock reserved for order {OrderId}", request.OrderId);
            return response;
        }
        catch (RpcException)
        {
            // Rollback transaction automatically
            await tx.RollbackAsync();
            _logger.LogError("Stock reservation failed for order {OrderId}", request.OrderId);
            throw;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Error reserving stock for order {OrderId}", request.OrderId);
            throw new RpcException(new Status(
            StatusCode.Internal,
            "Failed to reserve stock"
        ));
        }
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

    // Get all reservations for this order
    var reservations = await _context.StockReservations
        .Include(r => r.Inventory)
        .Where(r => r.OrderId == request.OrderId)
        .ToListAsync();

    if (reservations == null || reservations.Count == 0)
        throw new RpcException(new Status(StatusCode.NotFound, "Reservations not found"));

    bool anyUpdated = false;

    foreach (var reservation in reservations)
    {
        if (reservation.Status == ReservationStatus.Reserved)
        {
            // Restore stock
            reservation.Inventory.AvailableStock += reservation.ReservedQuantity;
            reservation.Status = ReservationStatus.Cancelled;
            anyUpdated = true;
        }
    }

    if (!anyUpdated)
        throw new RpcException(new Status(StatusCode.FailedPrecondition, "No reservations can be cancelled"));

    await _context.SaveChangesAsync();
    await tx.CommitAsync();

    return new CancelReservationResponse
    {
        Success = true
    };
}


   
}
