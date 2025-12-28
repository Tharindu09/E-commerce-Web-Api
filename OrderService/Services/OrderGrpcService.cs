using System;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.Services;

public class OrderGrpcService : Grpc.OrderService.OrderServiceBase
{
    private readonly AppDbContext _db;
    public OrderGrpcService(AppDbContext db)
    {
        _db = db;
    }

    public override async Task<Grpc.OrderResponse> GetOrderById(Grpc.OrderRequest request, ServerCallContext context)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o=>o.Id == request.OrderId);

        if (order == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Order with ID {request.OrderId} not found."));
        }

        var response = new Grpc.OrderResponse
        {
            OrderId = order.Id,
            UserName = order.UserName,
            TotalAmount = (double)order.TotalAmount,
            OrderStatus = order.OrderStatus

        };


        return response;
    }
}
