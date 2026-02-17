using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omar.Data;
using Omar.Dtos.ProductDto;
using Omar.Eunm;

namespace Omar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly AddDbContext _context;
        public ProductsController(AddDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Models.Products
            {
                Name = dto.Name,
                Category = dto.Category,
                SaleType = dto.SaleType,
                PricePerKg = dto.PricePerKg,
                PricePerPiece = dto.PricePerPiece,
                StockQuantity = dto.StockQuantity,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCreateDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = dto.Name;
            product.Category = dto.Category;
            product.SaleType = dto.SaleType;
            product.PricePerKg = dto.PricePerKg;
            product.PricePerPiece = dto.PricePerPiece;
            product.StockQuantity = dto.StockQuantity;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPut("update-stock/{id}")]
        public async Task<IActionResult> UpdateStock(int id, decimal quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            product.StockQuantity = quantity;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("deactivate/{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("ByCategory/{category}")]
        public async Task<IActionResult> GetProductsByCategory(Category category)
        {
            var products = await _context.Products
                .Where(p => p.Category == category && p.IsActive)
                .ToListAsync();

            return Ok(products);
        }

    }
}
