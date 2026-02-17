using System.ComponentModel.DataAnnotations.Schema;

namespace Omar.Models
{
    public class SaleItems
    {
        public int Id { get; set; }

        public int SaleId { get; set; }
        public Sales Sale { get; set; } = null!;

        public int ProductId { get; set; }
        public Products Product { get; set; } = null!;

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        // سعر البيع (اللي بعنا بيه للزبون)
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        // تكلفة الشراء (عشان نحسب الربح)
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        // الإجمالي (كمية * سعر بيع)
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
    }
}
