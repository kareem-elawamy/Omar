using System.ComponentModel.DataAnnotations;

namespace Omar.Dtos.SaleDto
{
    public class SaleCreateDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Sale must contain at least one item")]
        public List<SaleItemCreateDto> Items { get; set; } = new();

        [Range(0, double.MaxValue, ErrorMessage = "Paid amount cannot be negative")]
        public decimal PaidAmount { get; set; }
    }
}
