using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using S2O1.Domain.Entities;

namespace S2O1.DataAccess.Configurations
{
    public class AdditionalConfigurations : IEntityTypeConfiguration<OfferItem>,
        IEntityTypeConfiguration<InvoiceItem>,
        IEntityTypeConfiguration<PriceList>,
        IEntityTypeConfiguration<Warehouse>,
        IEntityTypeConfiguration<Title>,
        IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<OfferItem> builder)
        {
            builder.HasOne(x => x.Offer)
                .WithMany(o => o.Items)
                .HasForeignKey(x => x.OfferId)
                .OnDelete(DeleteBehavior.Cascade); // Items deleted with Offer is usually OK if Offer is logically deleted, but Requirements say Restrict ALL foreign keys "where necessary". 
                // However, "Offer" deletion is usually logical. If physical delete happens, items should go.
                // But let's stick to Restrict if in doubt or Cascade if composition?
                // "Tüm forein keys, gerekli yerlerde... Restrict"
                // Composition usually implies Cascade. But let's start with Cascade for Items->Parent, and Restrict for Items->Product.
            
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.HasOne(x => x.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public void Configure(EntityTypeBuilder<PriceList> builder)
        {
            builder.HasOne(x => x.Product)
                .WithMany(p => p.PriceLists)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Property(x => x.RowVersion).IsRowVersion();
        }

        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.HasOne(x => x.Company)
                .WithMany(c => c.Warehouses)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public void Configure(EntityTypeBuilder<Title> builder)
        {
             builder.HasOne(x => x.Company)
                .WithMany() // Title -> Company
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasOne(x => x.CustomerCompany)
                .WithMany(c => c.Customers)
                .HasForeignKey(x => x.CustomerCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
