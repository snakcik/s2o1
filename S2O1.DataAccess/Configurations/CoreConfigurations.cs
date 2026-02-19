using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using S2O1.Domain.Entities;

namespace S2O1.DataAccess.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Unique Code
            // Unique Code per Warehouse (to allow same product in different warehouses)
            builder.HasIndex(p => new { p.ProductCode, p.WarehouseId }).IsUnique(); 
            
            // Concurrency Token
            builder.Property(p => p.RowVersion).IsRowVersion();

            // Relations - Restrict Delete
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Brand)
                .WithMany() // Brand doesn't necessarily need a Products collection
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Unit)
                .WithMany()
                .HasForeignKey(p => p.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Warehouse)
                .WithMany()
                .HasForeignKey(p => p.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Shelf)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.ShelfId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.UserName).IsRequired().HasMaxLength(255);
            builder.Property(u => u.UserMail).IsRequired().HasMaxLength(255);
            
            // Indexes
            builder.HasIndex(u => u.UserName).IsUnique();
            builder.HasIndex(u => u.UserMail).IsUnique();

            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
    
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
             builder.HasOne(i => i.SellerCompany)
                .WithMany()
                .HasForeignKey(i => i.SellerCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
                
             builder.HasOne(i => i.BuyerCompany)
                .WithMany() // Invoice -> CustomerCompany
                .HasForeignKey(i => i.BuyerCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
    
    public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            builder.HasOne(m => m.Product)
                .WithMany()
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Warehouse)
                .WithMany(w => w.StockMovements)
                .HasForeignKey(m => m.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional relations can also be Restrict to prevent accidental cascade
            builder.HasOne(m => m.TargetWarehouse)
                .WithMany()
                .HasForeignKey(m => m.TargetWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
