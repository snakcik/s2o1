namespace S2O1.Business.Services.Interfaces
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int? WarehouseId { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UnitId { get; set; }
        public decimal CurrentStock { get; set; }
    }

    public class CreateProductDto
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int? WarehouseId { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UnitId { get; set; }
        public decimal InitialStock { get; set; }
    }

    public class UpdateProductDto : CreateProductDto
    {
        public int Id { get; set; }
        public decimal AddedStock { get; set; }
    }
}
