using System;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Dtos;

public class ProductCreateDto
{
    [Required]
    public string Name { get; set; } = default!;
    [Required]
    public string Description { get; set; } = default!;
    [Required]
    public decimal Price { get; set; }
    [Required]
    public string Category { get; set; } = default!;
    [Required]
    public int InitialStock { get; set; }
}

