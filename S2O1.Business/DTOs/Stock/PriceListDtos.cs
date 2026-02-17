namespace S2O1.Business.DTOs.Stock
{
    public class PriceListDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal DiscountRate { get; set; }
        public int VatRate { get; set; }
        public string Currency { get; set; }
        public bool IsActivePrice { get; set; }
    }

    public class CreatePriceListDto
    {
        public int ProductId { get; set; }
        public int? SupplierId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal DiscountRate { get; set; }
        public int VatRate { get; set; }
        public string Currency { get; set; }
        public bool IsActivePrice { get; set; }
    }

    public class UpdatePriceListDto : CreatePriceListDto
    {
        public int Id { get; set; }
    }
}
