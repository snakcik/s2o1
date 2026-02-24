namespace S2O1.Business.DTOs.Stock
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string BrandName { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class UnitDto
    {
        public int Id { get; set; }
        public string UnitName { get; set; }
        public string UnitShortName { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateBrandDto { public string BrandName { get; set; } public string BrandDescription { get; set; } }
    public class UpdateBrandDto : CreateBrandDto { public int Id { get; set; } }

    public class CreateCategoryDto { public string CategoryName { get; set; } public string CategoryDescription { get; set; } public int? ParentCategoryId { get; set; } }
    public class UpdateCategoryDto : CreateCategoryDto { public int Id { get; set; } }

    public class CreateUnitDto { public string UnitName { get; set; } public string UnitShortName { get; set; } public bool IsDecimal { get; set; } }
    public class UpdateUnitDto : CreateUnitDto { public int Id { get; set; } }
}
