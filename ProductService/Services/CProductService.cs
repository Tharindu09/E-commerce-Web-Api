using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Mapper;
using ProductService.Model;

namespace ProductService.Services;

public class CProductService : IProductService
{
    private readonly AppDbContext _context;
    public CProductService(AppDbContext context)
    {
        _context = context;
    }

    // CREATE PRODUCT
    public async Task<Product> CreateProductAsync(ProductCreateDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Category = dto.Category
        };

        // Create inventory row
        var inventory = new Inventory
        {
            TotalStock = dto.InitialStock,
            AvailableStock = dto.InitialStock,
            Product = product
        };

        _context.Products.Add(product);
        _context.Inventories.Add(inventory);

        await _context.SaveChangesAsync();
        return product;
    }

    // GET ALL
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Inventory)
            .ToListAsync();
    }

    // GET BY ID
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
