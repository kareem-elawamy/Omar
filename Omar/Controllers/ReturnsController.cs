using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omar.Data;
using Omar.Eunm;
using Omar.Models;

namespace Omar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Employee,Owner")] // الموظف يقدر يعمل مرتجع
    public class ReturnsController : ControllerBase
    {
        private readonly AddDbContext _context;

        public ReturnsController(AddDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ReturnItem(int saleId, int productId, decimal quantity)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // 1. هات الفاتورة والأصناف
                var sale = await _context
                    .Sales.Include(s => s.Items)
                    .FirstOrDefaultAsync(s => s.Id == saleId);

                if (sale == null)
                    return NotFound("Sale invoice not found");

                // 2. هات الصنف من الفاتورة
                var saleItem = sale.Items.FirstOrDefault(i => i.ProductId == productId);
                if (saleItem == null)
                    return BadRequest("This product is not in this sale");

                // 3. اتأكد إن الكمية المرتجعة مش أكبر من اللي اشتراها
                if (quantity > saleItem.Quantity)
                    return BadRequest("Cannot return more than sold quantity");

                // 4. هات المنتج الأصلي عشان نرجعله المخزون
                var product = await _context.Products.FindAsync(productId);

                // 5. الحسابات
                // بنحسب المبلغ اللي هنرجعه للزبون بناءً على سعر البيع وقت الفاتورة
                decimal refundAmount = quantity * saleItem.SellingPrice;

                // تحديث الفاتورة (بنقلل الإجمالي)
                sale.TotalAmount -= refundAmount;
                sale.PaidAmount -= refundAmount; // بنفترض إننا رجعنا فلوس كاش

                // تحديث البند في الفاتورة (أو حذفه لو رجع الكمية كلها)
                saleItem.Quantity -= quantity;
                saleItem.Total -= refundAmount;

                if (saleItem.Quantity == 0)
                {
                    _context.SaleItems.Remove(saleItem); // لو رجع كل حاجة شيل السطر ده
                }

                // 6. المخزون: رجع البضاعة للرف
                product.StockQuantity += quantity;

                // 7. سجل حركة مخزون (دخول - مرتجع)
                _context.StockMovements.Add(
                    new StockMovements
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        MovementType = MovementType.دخول, // رجعت المخزن تاني
                        Note = $"Return from Sale #{saleId} by {userId}",
                    }
                );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(
                    new { Message = "Item returned successfully", RefundAmount = refundAmount }
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
    }
}
