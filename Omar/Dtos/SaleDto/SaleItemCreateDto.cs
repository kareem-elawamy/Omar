using System.ComponentModel.DataAnnotations;

namespace Omar.Dtos.SaleDto
{
    public class SaleItemCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; } // يقبل كسور عشان الوزن (مثلا 0.250 كيلو)
    }
}
