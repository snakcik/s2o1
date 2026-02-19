using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using S2O1.Domain.Entities;

namespace S2O1.DataAccess.Configurations
{
    public class WarehouseShelfConfiguration : IEntityTypeConfiguration<WarehouseShelf>
    {
        public void Configure(EntityTypeBuilder<WarehouseShelf> builder)
        {
            builder.HasOne(s => s.Warehouse)
                .WithMany(w => w.Shelves)
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DispatchNoteConfiguration : IEntityTypeConfiguration<DispatchNote>
    {
        public void Configure(EntityTypeBuilder<DispatchNote> builder)
        {
            builder.HasOne(d => d.Company)
                .WithMany()
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Customer)
                .WithMany()
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class DispatchNoteItemConfiguration : IEntityTypeConfiguration<DispatchNoteItem>
    {
        public void Configure(EntityTypeBuilder<DispatchNoteItem> builder)
        {
            builder.Property(i => i.Quantity).HasColumnType("decimal(18,2)");

            builder.HasOne(i => i.DispatchNote)
                .WithMany(d => d.Items)
                .HasForeignKey(i => i.DispatchNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
