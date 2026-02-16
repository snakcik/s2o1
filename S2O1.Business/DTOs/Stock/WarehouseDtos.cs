namespace S2O1.Business.Services.Interfaces
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string WarehouseName { get; set; }
        public string Location { get; set; }
        public int CompanyId { get; set; }
    }

    public class CreateWarehouseDto
    {
        public string WarehouseName { get; set; }
        public string Location { get; set; }
        public int CompanyId { get; set; }
    }

    public class UpdateWarehouseDto : CreateWarehouseDto
    {
        public int Id { get; set; }
    }
}
