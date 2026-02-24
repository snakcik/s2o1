using S2O1.Domain.Enums;
using System;
using System.Collections.Generic;

namespace S2O1.Business.DTOs.Invoice
{
    public class CreateInvoiceDto
    {
        public int? OfferId { get; set; } // If created from offer
        public int SenderCompanyId { get; set; }
        public int ReceiverCompanyId { get; set; }
        // Invoice Items if not from offer or additional items
        public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
        public DateTime DueDate { get; set; }
        public int PreparedByUserId { get; set; }
    }

    public class InvoiceDto
    {
        public int Id { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal GrandTotal { get; set; }
        public InvoiceStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public List<InvoiceItemDto>? Items { get; set; }
        
        public int? AssignedDelivererUserId { get; set; }
        public string? AssignedDelivererUserName { get; set; }
        public string? ReceiverName { get; set; }
        
        // Customer Info
        public int BuyerCompanyId { get; set; }
        public string? BuyerCompanyName { get; set; }
        public string? BuyerCompanyAddress { get; set; }
        public string? BuyerCompanyTaxInfo { get; set; }
    }

    public class InvoiceItemDto
    {
        public int Id { get; set; } // Added for referencing specific item
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public string? WarehouseName { get; set; }
        public string? ShelfName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int VatRate { get; set; }
        public bool IncludeInDispatch { get; set; }
    }

    public class WarehouseDeliveryDto
    {
        public int InvoiceId { get; set; }
        public int DelivererUserId { get; set; }
        public string ReceiverName { get; set; }
        public List<int> IncludedItemIds { get; set; }
    }
}
