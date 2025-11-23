
using ProductService.Dtos;
using ProductService.Model;

namespace ProductService.Services;

public interface IProductService
{
    Task<Product> CreateProductAsync(ProductCreateDto dto);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
}
