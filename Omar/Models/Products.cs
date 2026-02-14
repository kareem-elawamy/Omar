using Omar.Eunm;
using System.Net.ServerSentEvents;

namespace Omar.Models
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Category Category { get; set; }
        public SaleType SaleType { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? PricePerPiece { get; set; }
        public decimal StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<SaleItems> SaleItems { get; set; } = new List<SaleItems>();
        public ICollection<StockMovements> StockMovements { get; set; } = new List<StockMovements>();


    }
}
