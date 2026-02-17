namespace Omar.Dtos.SaleDto
{
    public class SaleItemInvoiceDto
    {
        public string ProductName { get; set; } = null!;
        public decimal Quantity { get; set; }

        // غيرنا الاسم لـ SellingPrice عشان الاتساق
        public decimal SellingPrice { get; set; }

        public decimal Total { get; set; }
    }
}
