using S2O1.Domain.Enums;
using System;
using System.Collections.Generic;

namespace S2O1.Business.DTOs.Stock // Using Stock for now as mostly related
{
    public class OfferDto
    {
        public int Id { get; set; }
        public string OfferNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OfferDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; }
        public OfferStatus Status { get; set; }
        public List<OfferItemDto> Items { get; set; }
    }

    public class OfferItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountRate { get; set; }
        public string Currency { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CreateOfferDto
    {
        public int CustomerId { get; set; }
        public DateTime ValidUntil { get; set; }
        public List<CreateOfferItemDto> Items { get; set; } = new List<CreateOfferItemDto>();
    }

    public class CreateOfferItemDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountRate { get; set; }
        public string Currency { get; set; } = "TL";
    }

    public class SendOfferEmailDto
    {
        public string ToEmail { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
    }
}
