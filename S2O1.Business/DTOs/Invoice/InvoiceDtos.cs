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
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal GrandTotal { get; set; }
        public InvoiceStatus Status { get; set; }
        public List<InvoiceItemDto> Items { get; set; }
    }

    public class InvoiceItemDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int VatRate { get; set; }
    }
}
