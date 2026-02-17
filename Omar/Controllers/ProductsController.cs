using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omar.Data;
using Omar.Dtos.ProductDto;
using Omar.Eunm;
using Omar.Models;

namespace Omar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AddDbContext _context;

        public ProductsController(AddDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. أهم دالة للكاشير: البحث بالباركود
        // ==========================================
        // يستقبل الباركود ويرجع المنتج وسعره جاهز للفاتورة
        [HttpGet("scan")]
        public async Task<IActionResult> GetProductByBarcode([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Barcode is empty");

            // بنبحث عن الباركود ونتأكد إن المنتج نشط
            var product = await _context
                .Products.Where(p => p.Barcode == code && p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.SaleType,
                    // بنرجع السعر المناسب حسب النوع عشان الفرونت ميتعبش
                    // لو وزن يرجع سعر الكيلو، لو قطعة يرجع سعر القطعة
                    SellingPrice = p.SaleType == SaleType.الوزن ? p.PricePerKg : p.PricePerPiece,
                    p.StockQuantity, // عشان التنبيه لو الكمية قليلة
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { Message = "Product not found" });

            return Ok(product);
        }

        // ==========================================
        // 2. عرض كل المنتجات
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

            return Ok(products);
        }

        // ==========================================
        // 3. إضافة منتج جديد (مع الباركود وسعر الشراء)
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // التحقق من تكرار الباركود (لو المستخدم دخل باركود)
            if (!string.IsNullOrEmpty(dto.Barcode))
            {
                var barcodeExists = await _context.Products.AnyAsync(p => p.Barcode == dto.Barcode);
                if (barcodeExists)
                    return BadRequest("This barcode already exists for another product.");
            }

            var product = new Products
            {
                Name = dto.Name,
                Barcode = dto.Barcode, // حفظ الباركود
                Category = dto.Category,
                SaleType = dto.SaleType,

                // حفظ سعر الشراء (لتقارير الربح)
                BuyingPrice = dto.BuyingPrice,

                PricePerKg = dto.PricePerKg,
                PricePerPiece = dto.PricePerPiece,
                StockQuantity = dto.StockQuantity,
                IsActive = true,
            };

            _context.Products.Add(product);

            // تسجيل حركة مخزن (أرصدة افتتاحية)
            _context.StockMovements.Add(
                new StockMovements
                {
                    Product = product, // هيأخد الـ Id تلقائي بعد الحفظ
                    Quantity = dto.StockQuantity,
                    MovementType = MovementType.دخول,
                    Note = "Initial Stock (New Product)",
                }
            );

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // ==========================================
        // 4. تعديل منتج (تحديث الباركود والأسعار)
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCreateDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            // التحقق من تكرار الباركود (مع استثناء المنتج الحالي)
            if (!string.IsNullOrEmpty(dto.Barcode))
            {
                var barcodeExists = await _context.Products.AnyAsync(p =>
                    p.Barcode == dto.Barcode && p.Id != id
                ); // p.Id != id مهمة جداً

                if (barcodeExists)
                    return BadRequest("This barcode is already assigned to another product.");
            }

            product.Name = dto.Name;
            product.Barcode = dto.Barcode; // تحديث الباركود
            product.Category = dto.Category;
            product.SaleType = dto.SaleType;

            // تحديث سعر الشراء
            product.BuyingPrice = dto.BuyingPrice;

            product.PricePerKg = dto.PricePerKg;
            product.PricePerPiece = dto.PricePerPiece;

            // ملحوظة: لا نعدل الكمية من هنا مباشرة للحفاظ على دقة الجرد
            // الكمية تتعدل فقط من دالة التوريد (AddStock) أو الجرد

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Product updated successfully" });
        }

        // ==========================================
        // 5. توريد بضاعة للمخزن
        // ==========================================
        [HttpPut("add-stock/{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> AddStock(int id, decimal quantity)
        {
            if (quantity <= 0)
                return BadRequest("Quantity must be greater than 0");

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            // زودنا الكمية
            product.StockQuantity += quantity;

            // تسجيل حركة دخول للمخزن (عشان التقارير)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.StockMovements.Add(
                new StockMovements
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    MovementType = MovementType.دخول,
                    Note = $"Stock added by Owner ({userId})",
                }
            );

            await _context.SaveChangesAsync();
            return Ok(
                new { Message = "Stock added successfully", NewQuantity = product.StockQuantity }
            );
        }

        // ==========================================
        // 6. حذف منتج (Deactivate)
        // ==========================================
        [HttpPatch("deactivate/{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // 7. فلترة بالتصنيف
        // ==========================================
        [HttpGet("ByCategory/{category}")]
        public async Task<IActionResult> GetProductsByCategory(Category category)
        {
            var products = await _context
                .Products.Where(p => p.Category == category && p.IsActive)
                .ToListAsync();

            return Ok(products);
        }
    }
}
