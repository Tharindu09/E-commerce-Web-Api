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
public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto dto)
{
    if (dto == null)
        return BadRequest("Request body is required.");

    if (!ModelState.IsValid)
        return ValidationProblem(ModelState);

    if (dto.InitialStock < 0)
        return BadRequest("Initial stock cannot be negative.");

    try
    {
        var product = await _productService.CreateProductAsync(dto);

        var readDto = new ProductReadDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Category = product.Category,
            Stock = product.Inventory?.TotalStock ?? 0,
            ImageUrl = product.ImageUrl
        };

        return CreatedAtAction(nameof(GetProductById), new { id = readDto.Id }, readDto);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}

    // GET ALL PRODUCTS
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetAll()
    {
        try
        {
                var products = await _productService.GetAllProductsAsync();
    
                var result = products.Select(p => new ProductReadDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Category = p.Category,
                    Stock = p.Inventory.AvailableStock,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description
                }).ToList();
    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
        }
        
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductReadDto>> GetProductById(int id)
    {   try
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
                Stock = product.Inventory.AvailableStock,
                ImageUrl = product.ImageUrl,
                Description = product.Description
            };

            return Ok(readDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
       
    }

    //Test endpoint to return 500 error
    [HttpGet("error")]
    public IActionResult GetError()
    {   return StatusCode(500, "This is a test error message.");
    }
}
