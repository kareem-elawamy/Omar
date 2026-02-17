using System.ComponentModel.DataAnnotations;
using Omar.Eunm;

namespace Omar.Dtos.ProductDto
{
    public class ProductCreateDto
    {
        [Required]
        public string Name { get; set; } = String.Empty;
        public string? Barcode { get; set; }
        public Category Category { get; set; }

        public SaleType SaleType { get; set; }

        // التعديل الجديد: سعر الشراء ضروري
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Buying price must be greater than 0")]
        public decimal BuyingPrice { get; set; }

        public decimal? PricePerKg { get; set; }
        public decimal? PricePerPiece { get; set; }

        [Required]
        public decimal StockQuantity { get; set; }
    }
}
