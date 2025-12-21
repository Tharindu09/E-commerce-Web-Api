using System.ComponentModel.DataAnnotations;

namespace OrderService.Model;

public class Order
{   [Key]
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }

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

    //Order and Payment status
    [Required]
    public string OrderStatus { get; set; }

    //Time metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItem> Items { get; set; }

}


public enum OrderStatus
{   Draft,
    Created,
    PendingPayment,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

