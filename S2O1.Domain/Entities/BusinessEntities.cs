using S2O1.Domain.Common;
using S2O1.Domain.Enums;
using System;
using System.Collections.Generic;

namespace S2O1.Domain.Entities
{
    public class CustomerCompany : BaseEntity
    {
        public string CustomerCompanyName { get; set; }
        public string CustomerCompanyAddress { get; set; }
        public string CustomerCompanyMail { get; set; }
        public ICollection<Customer> Customers { get; set; }
    }

    public class Customer : BaseEntity
    {
        public int CustomerCompanyId { get; set; }
        public CustomerCompany CustomerCompany { get; set; }
        
        public string CustomerContactPersonName { get; set; }
        public string CustomerContactPersonLastName { get; set; }
        public string CustomerContactPersonMobilPhone { get; set; }
        public string CustomerContactPersonMail { get; set; }
    }

    public class Supplier : BaseEntity
    {
        public string SupplierCompanyName { get; set; }
        public string SupplierContactName { get; set; }
        public string SupplierContactMail { get; set; }
        public string SupplierAddress { get; set; }
    }

    public class Offer : BaseEntity, IConcurrencyHandled
    {
        public string OfferNumber { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        
        public DateTime OfferDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal TotalAmount { get; set; }
        public OfferStatus Status { get; set; }
        
        public byte[] RowVersion { get; set; }

        public ICollection<OfferItem> Items { get; set; }
    }

    public class OfferItem : BaseEntity
    {
        public int OfferId { get; set; }
        public Offer Offer { get; set; }
        
        public int ProductId { get; set; }
        public Product Product { get; set; }
        
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountRate { get; set; }
    }

    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; }
        
        public int? OfferId { get; set; } // Optional if not from offer
        public Offer Offer { get; set; }
        
        public int SellerCompanyId { get; set; }
        public Company SellerCompany { get; set; }
        
        public int BuyerCompanyId { get; set; } // Could link to CustomerCompany
        public CustomerCompany BuyerCompany { get; set; }
        
        public int PreparedByUserId { get; set; }
        public User PreparedByUser { get; set; }
        
        public int ApprovedByUserId { get; set; }
        public User ApprovedByUser { get; set; }
        
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }
        
        public Guid? EInvoiceUuid { get; set; }
        public InvoiceStatus Status { get; set; }
        
        public ICollection<InvoiceItem> Items { get; set; }
    }

    public class InvoiceItem : BaseEntity
    {
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        
        public int ProductId { get; set; }
        public Product Product { get; set; }
        
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int VatRate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
