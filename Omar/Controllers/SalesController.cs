using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omar.Data;
using Omar.Dtos.SaleDto;
using Omar.Eunm;
using Omar.Models;

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

        // ==========================================
        // 1. إنشاء عملية بيع جديدة (مع تسجيل حركة المخزون والأسعار)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] SaleCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var sale = new Sales
                {
                    SaleDate = DateTime.Now,
                    PaidAmount = dto.PaidAmount,
                    UserId = userId!,
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;

                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);

                    // Validation
                    if (product == null || !product.IsActive)
                        return BadRequest($"Product ID {item.ProductId} not found or inactive");

                    if (product.StockQuantity < item.Quantity)
                        return BadRequest($"Not enough stock for product: {product.Name}");

                    // تحديد سعر البيع بناء على النوع
                    decimal sellingPrice =
                        product.SaleType == SaleType.الوزن
                            ? product.PricePerKg!.Value
                            : product.PricePerPiece!.Value;

                    decimal itemTotal = sellingPrice * item.Quantity;
                    totalAmount += itemTotal;

                    // تسجيل الصنف في الفاتورة
                    var saleItem = new SaleItems
                    {
                        SaleId = sale.Id,
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        SellingPrice = sellingPrice, // سعر البيع للزبون
                        PurchasePrice = product.BuyingPrice, // تكلفة الشراء (لحساب الربح)
                        Total = itemTotal,
                    };
                    _context.SaleItems.Add(saleItem);

                    // خصم المخزون
                    product.StockQuantity -= item.Quantity;

                    // تسجيل حركة مخزون (خروج)
                    _context.StockMovements.Add(
                        new StockMovements
                        {
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            MovementType = MovementType.خروج,
                            Note = $"Sold by user {userId} (Sale #{sale.Id})",
                        }
                    );
                }

                sale.TotalAmount = totalAmount;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(
                    new
                    {
                        sale.Id,
                        sale.TotalAmount,
                        sale.PaidAmount,
                        Message = "Sale completed successfully",
                    }
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Sale failed: {ex.Message}");
            }
        }

        // ==========================================
        // 2. عرض كل المبيعات (للمالك فقط) - مع Pagination
        // ==========================================
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetAllSales(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            // التأكد إن القيم منطقية
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 10;

            var query = _context.Sales.AsQueryable();

            // 1. حساب العدد الكلي (عشان الفرونت إند يعرف فيه كام صفحة)
            var totalCount = await query.CountAsync();

            // 2. جلب البيانات للصفحة المطلوبة فقط
            var sales = await query
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .Include(s => s.User)
                .OrderByDescending(s => s.SaleDate)
                .Skip((pageNumber - 1) * pageSize) // تخطي الصفحات السابقة
                .Take(pageSize) // أخذ العدد المطلوب
                .Select(s => new
                {
                    s.Id,
                    EmployeeName = s.User.UserName,
                    s.SaleDate,
                    s.TotalAmount,
                    ItemCount = s.Items.Count,
                })
                .ToListAsync();

            return Ok(
                new
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Data = sales,
                }
            );
        }

        // ==========================================
        // 3. عرض مبيعاتي (للموظف) - مع Pagination
        // ==========================================
        [HttpGet("mine")]
        [Authorize(Roles = "Employee,Owner")]
        public async Task<IActionResult> GetMySales(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 10;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Sales.Where(s => s.UserId == userId);

            var totalCount = await query.CountAsync();

            var sales = await query
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(s => s.SaleDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.Id,
                    s.SaleDate,
                    s.TotalAmount,
                    ItemCount = s.Items.Count,
                })
                .ToListAsync();

            return Ok(
                new
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Data = sales,
                }
            );
        }

        // ==========================================
        // 4. عرض تفاصيل فاتورة واحدة (للطباعة)
        // ==========================================
        [HttpGet("{id}/invoice")]
        [Authorize(Roles = "Employee,Owner")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var sale = await _context
                .Sales.Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
                return NotFound(new { Message = "Invoice not found" });

            // مسموح للموظف يشوف فواتيره، والمالك يشوف كله
            // (اختياري: لو عايز تمنع موظف يشوف فاتورة موظف تاني)
            /*
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = User.IsInRole("Owner");
            if (!isOwner && sale.UserId != currentUserId)
                return Forbid();
            */

            var invoice = new SaleInvoiceDto
            {
                SaleId = sale.Id,
                EmployeeName = sale.User.UserName!,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                PaidAmount = sale.PaidAmount,
                Items = sale
                    .Items.Select(i => new SaleItemInvoiceDto
                    {
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        SellingPrice = i.SellingPrice, // استخدام المسمى الجديد
                        Total = i.Total,
                    })
                    .ToList(),
            };

            return Ok(invoice);
        }
    }
}
