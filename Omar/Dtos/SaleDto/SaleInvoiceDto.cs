namespace Omar.Dtos.SaleDto
{
    public class SaleInvoiceDto
    {
        public int SaleId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public DateTime SaleDate { get; set; }
        public List<SaleItemInvoiceDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
    }
}
