using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omar.Data;
using Omar.Dtos.SaleDto;
using Omar.Eunm;
using Omar.Models;
using System.Security.Claims;

namespace Omar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly AddDbContext _context;

        public SalesController(AddDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] SaleCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var sale = new Sales
                {
                    SaleDate = DateTime.Now,
                    PaidAmount = dto.PaidAmount,
                    UserId = userId // ربط الفاتورة بالمستخدم
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;

                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || !product.IsActive)
                        return BadRequest($"Product {item.ProductId} not found or inactive");

                    if (product.StockQuantity < item.Quantity)
                        return BadRequest($"Not enough stock for {product.Name}");

                    decimal price = product.SaleType == SaleType.الوزن
                        ? product.PricePerKg!.Value
                        : product.PricePerPiece!.Value;

                    decimal itemTotal = price * item.Quantity;
                    totalAmount += itemTotal;

                    // Add SaleItem
                    var saleItem = new SaleItems
                    {
                        SaleId = sale.Id,
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        Price = price,
                        Total = itemTotal
                    };
                    _context.SaleItems.Add(saleItem);

                    // Update Stock
                    product.StockQuantity -= item.Quantity;

                    // Record Stock Movement
                    _context.StockMovements.Add(new StockMovements
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        MovementType = MovementType.خروج,
                        Note = $"Sold by user {userId}"
                    });
                }

                sale.TotalAmount = totalAmount;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    sale.Id,
                    sale.TotalAmount,
                    sale.PaidAmount
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Sale failed: {ex.Message}");
            }
        }

        // =========================
        // Get all sales (Owner only)
        // =========================
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetAllSales()
        {
            var sales = await _context.Sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .Include(s => s.User)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return Ok(sales);
        }

        // =========================
        // Get my sales (Employee)
        // =========================
        [HttpGet("mine")]
        [Authorize(Roles = "Employee,Owner")]
        public async Task<IActionResult> GetMySales()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sales = await _context.Sales
                .Where(s => s.UserId == userId)
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return Ok(sales);
        }
        [HttpGet("{id}/invoice")]
        [Authorize(Roles = "Employee,Owner")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound();

            var invoice = new SaleInvoiceDto
            {
                SaleId = sale.Id,
                EmployeeName = sale.User.UserName,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                PaidAmount = sale.PaidAmount,
                Items = sale.Items.Select(i => new SaleItemInvoiceDto
                {
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Total = i.Total
                }).ToList()
            };

            return Ok(invoice);
        }

    }
}
