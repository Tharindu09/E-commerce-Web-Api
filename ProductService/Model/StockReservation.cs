using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Model;

public class StockReservation
{   [Key]
    public int Id { get; set; }
    [ForeignKey("Inventory")]
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int OrderId { get; set; }
    public int ReservedQuantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ReservationStatus Status { get; set; }
    public Inventory Inventory
    { get; set; } // Navigation property to Inventory

}

public enum ReservationStatus
{
    Reserved,
    Confirmed,
    Cancelled,
    Expired
}
