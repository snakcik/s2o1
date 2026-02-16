using System.Collections.Generic;

namespace S2O1.Business.DTOs.Stock
{
    public class WarehouseStockReportDto
    {
        public string WarehouseName { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal CurrentStock { get; set; }
        public string UnitName { get; set; }
    }
}
