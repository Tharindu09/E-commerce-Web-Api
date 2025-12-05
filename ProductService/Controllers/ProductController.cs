using Microsoft.AspNetCore.Mvc;
using ProductService.Dtos;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // CREATE PRODUCT
    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _productService.CreateProductAsync(dto);

        var readDto = new ProductReadDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Category = product.Category,
            Stock = product.Inventory.Stock
        };

        return CreatedAtAction(nameof(GetProductById), new { id = readDto.Id }, readDto);
    }

    // GET ALL PRODUCTS
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();

        var result = products.Select(p => new ProductReadDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Category = p.Category,
            Stock = p.Inventory.Stock
        });

        return Ok(result);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductReadDto>> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
            return NotFound("Product not found");

        var readDto = new ProductReadDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Category = product.Category,
            Stock = product.Inventory.Stock
        };

        return Ok(readDto);
    }
}
