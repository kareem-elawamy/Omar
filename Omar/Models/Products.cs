using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Omar.Eunm;

namespace Omar.Models
{
    [Index(nameof(Barcode), IsUnique = true)]
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // التصنيف (عطارة / بقالة)
        public Category Category { get; set; }

        // نوع البيع (وزن / قطعة)
        public SaleType SaleType { get; set; }

        // ==============================
        // الأسعار (شراء وبيع)
        // ==============================

        // سعر الشراء (للكيلو أو للقطعة حسب النوع) - ضروري لحساب الربح
        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyingPrice { get; set; }

        // سعر البيع (للكيلو)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PricePerKg { get; set; }
        public string? Barcode { get; set; } // يقبل Null عشان العطارة ممكن ملهاش باركود

        // سعر البيع (للقطعة)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PricePerPiece { get; set; }

        // ==============================
        // المخزون
        // ==============================
        [Column(TypeName = "decimal(18,3)")] // 3 أرقام عشرية عشان الميزان (مثلا 0.250 كجم)
        public decimal StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        // علاقات
        public ICollection<SaleItems> SaleItems { get; set; } = new List<SaleItems>();
        public ICollection<StockMovements> StockMovements { get; set; } =
            new List<StockMovements>();
    }
}
