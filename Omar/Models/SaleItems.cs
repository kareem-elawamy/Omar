namespace Omar.Models
{
    public class SaleItems
    {
        public int Id { get; set; }

        public int SaleId { get; set; }
        public Sales Sale { get; set; } = null!;

        public int ProductId { get; set; }
        public Products Product { get; set; } = null!;

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total { get; set; }
    }
}
