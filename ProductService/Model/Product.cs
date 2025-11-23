using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Model;

public class Product
{   [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = default!;
    [Required]
    public string Description { get; set; } = default!;
    [Required]
    public decimal Price { get; set; }
    [Required]
    public string Category { get; set; } = default!;
    
    // Relation
    public Inventory Inventory { get; set; }

}
