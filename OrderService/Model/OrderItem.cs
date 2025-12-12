using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Model;

public class OrderItem
{   [Key]
    public int Id { get; set; }
    [ForeignKey("Order")]
    public int OrderId { get; set; }

    public int ProductId { get; set; }
    
    // Snapshot of product details
    public string ProductName { get; set; } = "";
    public decimal PriceAtPurchase { get; set; }
    public int Quantity { get; set; }

    public decimal Total => PriceAtPurchase * Quantity;
    public Order Order { get; set; }
}
