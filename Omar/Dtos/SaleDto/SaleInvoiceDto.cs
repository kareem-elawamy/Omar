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

        // الباقي (عشان الكاشير يعرف يرجع كام للزبون)
        public decimal Change => PaidAmount > TotalAmount ? PaidAmount - TotalAmount : 0;

        // المتبقي (لو عليه فلوس - آجل)
        public decimal RemainingDebt => TotalAmount > PaidAmount ? TotalAmount - PaidAmount : 0;
    }
}
