using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Omar.Models
{
    public class Expenses
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = null!; // وصف المصروف (فاتورة كهرباء، أكياس، صيانة)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; } // المبلغ

        public DateTime Date { get; set; } = DateTime.Now; // تاريخ الصرف

        public string? UserId { get; set; } // مين اللي سجل المصروف (للمراجعة)
    }
}
