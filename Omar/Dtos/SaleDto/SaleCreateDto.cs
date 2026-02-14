namespace Omar.Dtos.SaleDto
{
    public class SaleCreateDto
    {
        public List<SaleItemCreateDto> Items { get; set; } = new();
        public decimal PaidAmount { get; set; }
    }
}
