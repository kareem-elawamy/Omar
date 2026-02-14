using Omar.Eunm;

namespace Omar.Dtos.ProductDto
{
    public class ProductCreateDto
    {
        public string  Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Category Category { get; set; }
        public SaleType SaleType { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? PricePerPiece { get; set; }
        public decimal StockQuantity { get; set; }
    }
}
