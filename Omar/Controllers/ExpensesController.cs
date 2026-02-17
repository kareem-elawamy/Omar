using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omar.Data;
using Omar.Models;

namespace Omar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly AddDbContext _context;

        public ExpensesController(AddDbContext context)
        {
            _context = context;
        }

        // تسجيل مصروف جديد (زي ما هي)
        [HttpPost]
        public async Task<IActionResult> AddExpense([FromBody] Expenses expense)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            expense.UserId = userId;
            expense.Date = DateTime.Now;

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Expense added successfully", expense.Id });
        }

        // ====================================================
        // التعديل: عرض المصروفات مع Pagination
        // ====================================================
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetExpenses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20
        )
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 20;

            var query = _context.Expenses.AsQueryable();

            // 1. إجمالي عدد المصروفات
            var totalCount = await query.CountAsync();

            // 2. جلب البيانات للصفحة المطلوبة
            var expenses = await query
                .OrderByDescending(e => e.Date) // الأحدث أولاً
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(
                new
                {
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Data = expenses,
                }
            );
        }

        // حذف مصروف (زي ما هي)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Expense deleted" });
        }
    }
}
