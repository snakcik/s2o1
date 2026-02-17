using S2O1.Domain.Common;
using S2O1.Domain.Enums;
using System.Collections.Generic;

namespace S2O1.Domain.Entities
{
    public class Warehouse : BaseEntity
    {
        public string WarehouseName { get; set; }
        public string Location { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public ICollection<ProductLocation> Locations { get; set; }
        public ICollection<StockMovement> StockMovements { get; set; }
    }

    public class ProductLocation : BaseEntity
    {
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public string LocationCode { get; set; }
        public string LocationDescription { get; set; }
    }

    public class Brand : BaseEntity
    {
        public string BrandName { get; set; }
        public string BrandDescription { get; set; }
        public byte[] BrandLogo { get; set; }
    }

    public class Category : BaseEntity
    {
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public int? ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    public class ProductUnit : BaseEntity
    {
        public string UnitName { get; set; }
        public string UnitShortName { get; set; }
        public bool IsDecimal { get; set; }
    }

    public class Product : BaseEntity, IConcurrencyHandled
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
        
        public int UnitId { get; set; }
        public ProductUnit Unit { get; set; }
        
        public int? WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        
        public int? LocationId { get; set; }
        public ProductLocation Location { get; set; }
        
        public decimal CurrentStock { get; set; } // Managed by StockMovement
        public string ImageUrl { get; set; }
        
        public byte[] RowVersion { get; set; } // Concurrency Token

        public ICollection<PriceList> PriceLists { get; set; }
        public ICollection<StockAlert> StockAlerts { get; set; }
    }

    public class PriceList : BaseEntity, IConcurrencyHandled
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        
        public int? SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal DiscountRate { get; set; }
        public int VatRate { get; set; }
        public string Currency { get; set; }
        public bool IsActivePrice { get; set; }
        
        public byte[] RowVersion { get; set; }
    }

    public class StockAlert : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public decimal MinStockLevel { get; set; }
        public decimal? MaxStockLevel { get; set; }
        public bool IsNotificationSent { get; set; }
    }

    public class StockMovement : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        
        public int? TargetWarehouseId { get; set; } // For Transfer
        public Warehouse TargetWarehouse { get; set; }
        
        public MovementType MovementType { get; set; }
        public decimal Quantity { get; set; }
        public DateTime MovementDate { get; set; }
        
        public string DocumentNo { get; set; }
        public string Description { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public int? SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        
        public int? CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
