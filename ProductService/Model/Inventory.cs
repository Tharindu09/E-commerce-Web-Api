using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Model;

public class Inventory
{   
    [Key]
    public int Id { get; set; }

    [ForeignKey("Product")]
    public int ProductId { get; set; }
    [Required]
    public int TotalStock { get; set; }

    public int AvailableStock { get; set; } 

    public Product Product { get; set; } // Navigation property to Product

    public List<StockReservation> StockReservations { get; set; } // Navigation property to StockReservation

}
