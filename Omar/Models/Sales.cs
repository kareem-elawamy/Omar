using System.Net.ServerSentEvents;

namespace Omar.Models
{
    public class Sales
    {
        public int Id { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<SaleItems> Items { get; set; } = new List<SaleItems>();

    }
}
