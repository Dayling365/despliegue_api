namespace LibreriaDonCesar.Core.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int CustomerTypeId { get; set; }
        public string? CustomerTypeName { get; set; }
    }
}
