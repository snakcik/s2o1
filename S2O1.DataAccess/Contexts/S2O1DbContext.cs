using Microsoft.EntityFrameworkCore;
using S2O1.Domain.Entities;
using Module = S2O1.Domain.Entities.Module; // Fix ambiguity with System.Reflection.Module
using S2O1.Domain.Common;
using S2O1.Domain.Common;
using System.Reflection;
using S2O1.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace S2O1.DataAccess.Contexts
{
    public class S2O1DbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public S2O1DbContext(DbContextOptions<S2O1DbContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        // Auth & System
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<UserApiKey> UserApiKeys { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<LicenseInfo> LicenseInfos { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Organization
        public DbSet<Company> Companies { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<ProductLocation> ProductLocations { get; set; }

        // Product & Stock
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductUnit> ProductUnits { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        // Business
        public DbSet<CustomerCompany> CustomerCompanies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OfferItem> OfferItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Global Query Filters
            // Apply to all entities inheriting from BaseEntity
            // We can do this via reflection or manual. Manual is safer/explicit.
            
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
            // ... (Can optimize this with reflection loop if allowed, otherwise explicit)
            // Requirements say: "Yetkisiz veri erişimini engellemek için IsDeleted = false ... içeren Global Sorgu Filtreleri"
            
            // Loop for Global Filter
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Using EF Core generic SetQueryFilter method dynamically might be complex here.
                    // For simplicity and robustness in this generated code, I'll rely on the specific Configurations 
                    // or implement a helper.
                    // Actually, let's put it in the Configuration classes or use a loop here.
                    
                    var method = SetGlobalQueryMethod.MakeGenericMethod(entityType.ClrType);
                    method.Invoke(this, new object[] { modelBuilder });
                }
            }

            // Fix cascade delete cycles for Invoice
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.PreparedByUser)
                .WithMany()
                .HasForeignKey(i => i.PreparedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.ApprovedByUser)
                .WithMany()
                .HasForeignKey(i => i.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }

        static readonly MethodInfo SetGlobalQueryMethod = typeof(S2O1DbContext)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(t => t.IsGenericMethod && t.Name == nameof(SetGlobalQuery));

        private void SetGlobalQuery<T>(ModelBuilder builder) where T : BaseEntity
        {
            builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted && e.IsActive);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
             var userId = _currentUserService.UserId;

             foreach (var entry in ChangeTracker.Entries<BaseEntity>())
             {
                 switch (entry.State)
                 {
                     case EntityState.Added:
                         entry.Entity.CreateDate = System.DateTime.Now;
                         entry.Entity.IsDeleted = false;
                         if(userId.HasValue) entry.Entity.CreatedByUserId = userId.Value;
                         break;
                     case EntityState.Modified:
                         if (userId.HasValue) entry.Entity.UpdatedByUserId = userId.Value;
                         break;
                 }
             }
             
             // Audit Log Logic
             var auditEntryPairs = new System.Collections.Generic.List<(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry, AuditLog Log)>();
             foreach (var entry in ChangeTracker.Entries<BaseEntity>())
             {
                 if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged) continue;
                 if (entry.Entity is AuditLog) continue; 

                 var audit = new AuditLog
                 {
                     ActionType = entry.State.ToString(),
                     EntityName = entry.Entity.GetType().Name,
                     CreateDate = System.DateTime.Now,
                     ActorUserId = userId,
                     ActorUserName = _currentUserService.UserName ?? "System",
                     ActorRole = _currentUserService.UserRole ?? "Unknown",
                     Source = _currentUserService.Source ?? "System",
                     IPAddress = _currentUserService.IpAddress ?? "::1",
                     EntityId = entry.Entity.Id.ToString(), // Will be 0 for Added
                     ActionDescription = $"{entry.State} operation on {entry.Entity.GetType().Name}"
                 };
                 auditEntryPairs.Add((entry, audit));
             }

             var result = await base.SaveChangesAsync(cancellationToken);

             if (auditEntryPairs.Any())
             {
                 foreach (var pair in auditEntryPairs)
                 {
                     // Update ID if it was 0 (Added)
                     if (pair.Log.ActionType == "Added")
                     {
                         pair.Log.EntityId = pair.Entry.Entity.GetType().GetProperty("Id")?.GetValue(pair.Entry.Entity)?.ToString() ?? pair.Log.EntityId;
                     }
                     AuditLogs.Add(pair.Log);
                 }
                 await base.SaveChangesAsync(cancellationToken);
             }
             
            return result;
        }
    }
}
