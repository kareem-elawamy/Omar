namespace Omar.Dtos.SaleDto
{
    public class SaleItemInvoiceDto
    {
        public string ProductName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }
}
