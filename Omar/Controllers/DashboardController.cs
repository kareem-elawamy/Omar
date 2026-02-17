using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omar.Data;
using Omar.Eunm;

namespace Omar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class DashboardController : ControllerBase
    {
        private readonly AddDbContext _context;

        public DashboardController(AddDbContext context)
        {
            _context = context;
        }

        // =================================================================
        // API التقرير الشامل (يقبل شهر وسنة)
        // =================================================================
        // Usage: api/Dashboard/summary?month=5&year=2024
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year)
        {
            // لو التاريخ مش مبعوت، استخدم تاريخ النهاردة
            var targetMonth = month ?? DateTime.Now.Month;
            var targetYear = year ?? DateTime.Now.Year;

            // تحديد بداية ونهاية الشهر المطلوب
            var startOfTargetMonth = new DateTime(targetYear, targetMonth, 1);
            var startOfNextMonth = startOfTargetMonth.AddMonths(1);

            // تحديد شهر المقارنة (الشهر السابق للشهر المختار)
            var startOfLastMonth = startOfTargetMonth.AddMonths(-1);

            // ---------------------------------------------------------
            // 1. المبيعات (Revenue) للشهر المختار
            // ---------------------------------------------------------
            var targetMonthSales = await _context
                .Sales.Where(s => s.SaleDate >= startOfTargetMonth && s.SaleDate < startOfNextMonth)
                .Include(s => s.Items)
                .ToListAsync();

            decimal totalRevenue = targetMonthSales.Sum(s => s.TotalAmount);

            // 2. تكلفة البضاعة المباعة (COGS)
            decimal cogs = targetMonthSales
                .SelectMany(s => s.Items)
                .Sum(i => i.Quantity * i.PurchasePrice);

            decimal grossProfit = totalRevenue - cogs;

            // ---------------------------------------------------------
            // 3. المصروفات (Expenses) للشهر المختار
            // ---------------------------------------------------------
            decimal totalExpenses = await _context
                .Expenses.Where(e => e.Date >= startOfTargetMonth && e.Date < startOfNextMonth)
                .SumAsync(e => e.Amount);

            // 4. صافي الربح
            decimal netProfit = grossProfit - totalExpenses;

            // ---------------------------------------------------------
            // 5. المقارنة بالشهر السابق
            // ---------------------------------------------------------

            // تكلفة التوريد (Restocking) في الشهر المختار
            var restockingCost = await _context
                .StockMovements.Where(m =>
                    m.MovementDate >= startOfTargetMonth
                    && m.MovementDate < startOfNextMonth
                    && m.MovementType == MovementType.دخول
                )
                .SumAsync(m => m.Quantity * m.Product.BuyingPrice);

            // مبيعات الشهر السابق (للمقارنة)
            decimal lastMonthRevenue = await _context
                .Sales.Where(s => s.SaleDate >= startOfLastMonth && s.SaleDate < startOfTargetMonth)
                .SumAsync(s => s.TotalAmount);

            double growthPercentage = 0;
            if (lastMonthRevenue > 0)
            {
                growthPercentage =
                    (double)((totalRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
            }

            return Ok(
                new
                {
                    Period = $"{targetMonth}/{targetYear}", // بنرجعله الفترة عشان يعرضها في العنوان

                    TotalRevenue = totalRevenue,
                    TotalExpenses = totalExpenses,
                    NetProfit = netProfit,

                    GrossProfit = grossProfit,
                    RestockingCost = restockingCost,

                    LastMonthRevenue = lastMonthRevenue,
                    GrowthPercentage = Math.Round(growthPercentage, 1),

                    InvoicesCount = targetMonthSales.Count,
                }
            );
        }

        // =================================================================
        // رسم بياني للمبيعات والمصروفات
        // =================================================================
        [HttpGet("chart-data")]
        public async Task<IActionResult> GetChartData()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // تجميع المبيعات يوم بيوم
            var salesData = await _context
                .Sales.Where(s => s.SaleDate >= startOfMonth)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(s => s.TotalAmount) })
                .ToListAsync();

            // تجميع المصروفات يوم بيوم
            var expensesData = await _context
                .Expenses.Where(e => e.Date >= startOfMonth)
                .GroupBy(e => e.Date.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(e => e.Amount) })
                .ToListAsync();

            return Ok(new { Sales = salesData, Expenses = expensesData });
        }

        // =================================================================
        // أعلى المنتجات مبيعاً
        // =================================================================
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var topProducts = await _context
                .SaleItems.Include(i => i.Sale)
                .Include(i => i.Product)
                .Where(i => i.Sale.SaleDate >= startOfMonth)
                .GroupBy(i => new { i.ProductId, i.Product.Name })
                .Select(g => new
                {
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Total),
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            return Ok(topProducts);
        }

        // =================================================================
        // تقرير حالة المخزون
        // =================================================================
        [HttpGet("inventory-status")]
        public async Task<IActionResult> GetInventoryStatus()
        {
            var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

            // قيمة المخزون بسعر الشراء (رأس المال المركون)
            decimal inventoryValue = products.Sum(p => p.StockQuantity * p.BuyingPrice);

            // النواقص
            var lowStockItems = products
                .Where(p => p.StockQuantity <= 5) // الحد الأدنى
                .Select(p => new
                {
                    p.Name,
                    p.StockQuantity,
                    p.Category,
                })
                .ToList();

            return Ok(
                new
                {
                    InventoryValue = inventoryValue,
                    LowStockCount = lowStockItems.Count,
                    LowStockItems = lowStockItems,
                }
            );
        }
    }
}
