using System;
using System.Collections.Generic;

namespace S2O1.Business.DTOs.Logistic
{
    public class DispatchNoteDto
    {
        public int Id { get; set; }
        public string DispatchNo { get; set; }
        public DateTime DispatchDate { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        
        public string DelivererName { get; set; }
        public string ReceiverName { get; set; }
        
        public IEnumerable<DispatchNoteItemDto> Items { get; set; }
    }

    public class DispatchNoteItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string BrandName { get; set; }
        public decimal Quantity { get; set; }
        public string UnitName { get; set; }
    }

    public class CreateDispatchNoteDto
    {
        public DateTime DispatchDate { get; set; }
        public int CompanyId { get; set; }
        public int? CustomerId { get; set; }
        public string DelivererName { get; set; }
        public string ReceiverName { get; set; }
        public string Note { get; set; }
        public List<CreateDispatchNoteItemDto> Items { get; set; }
    }

    public class CreateDispatchNoteItemDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
}
