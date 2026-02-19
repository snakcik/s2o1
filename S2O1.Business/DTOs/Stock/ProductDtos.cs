namespace S2O1.Business.Services.Interfaces
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string SystemCode { get; set; }
        public int? WarehouseId { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal ReservedStock { get; set; }
        public string Currency { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPhysical { get; set; }
        public int? ShelfId { get; set; }
        public string ShelfName { get; set; }
    }

    public class CreateProductDto
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string SystemCode { get; set; }
        public int? WarehouseId { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UnitId { get; set; }
        public decimal InitialStock { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPhysical { get; set; } = true;
        public int? ShelfId { get; set; }
    }

    public class UpdateProductDto : CreateProductDto
    {
        public int Id { get; set; }
        public decimal AddedStock { get; set; }
    }
}
