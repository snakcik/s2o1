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
        public DateTime OfferDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal TotalAmount { get; set; }
        public OfferStatus Status { get; set; }
        public List<OfferItemDto> Items { get; set; }
    }

    public class OfferItemDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountRate { get; set; }
    }
}
