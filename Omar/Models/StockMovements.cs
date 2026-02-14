using Omar.Eunm;

namespace Omar.Models
{
    public class StockMovements
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Products Product { get; set; } = null!;

        public decimal Quantity { get; set; }

        public MovementType MovementType { get; set; }
        // In - Out

        public DateTime MovementDate { get; set; } = DateTime.Now;

        public string? Note { get; set; }

    }
}
