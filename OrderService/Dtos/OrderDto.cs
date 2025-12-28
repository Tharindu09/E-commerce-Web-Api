using System;
using OrderService.Model;

namespace OrderService.Dtos;

public class OrderDto
{   
    public int Id { get; set;}

    // User snapshot
    public string UserName { get; set; } = "";
    public string UserPhone { get; set; } = "";
    public string UserEmail { get; set; } = "";

    public string ShipLine1 { get; set; } = "";
    public string ShipLine2 { get; set; } = "";
    public string ShipCity { get; set; } = "";
    public string ShipDistrict { get; set; } = "";
    public string ShipProvince { get; set; } = "";
    public string ShipPostalCode { get; set; } = "";
    public decimal TotalAmount { get; set; }

    public string OrderStatus { get; set; } 

    public List<OrderItemDto> Items { get; set; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    
    // Snapshot of product details
    public string ProductName { get; set; } = "";
    public decimal PriceAtPurchase { get; set; }
    public int Quantity { get; set; }

    public decimal Total => PriceAtPurchase * Quantity;

}

