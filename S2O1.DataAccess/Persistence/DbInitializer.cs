using Microsoft.EntityFrameworkCore;
using S2O1.Core.Security;
using S2O1.DataAccess.Contexts;
using S2O1.Domain.Entities;
using Module = S2O1.Domain.Entities.Module; // Alias to resolve ambiguity
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace S2O1.DataAccess.Persistence
{
    public class DbInitializer
    {
        private readonly S2O1DbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public DbInitializer(S2O1DbContext context, IPasswordHasher passwordHasher)
        {
             _context = context;
            _passwordHasher = passwordHasher;
        }

        public static async Task InitializeAsync(S2O1DbContext context, IServiceProvider provider)
        {
            var passwordHasher = provider.GetRequiredService<IPasswordHasher>();
            var initializer = new DbInitializer(context, passwordHasher);
            
            // Scan relevant assemblies for modules
            var assemblies = new List<Assembly>();
            if (Assembly.GetEntryAssembly() != null) assemblies.Add(Assembly.GetEntryAssembly());
            
            // Add Domain assembly to scan for Entities
            var domainAssembly = typeof(S2O1.Domain.Common.BaseEntity).Assembly;
            assemblies.Add(domainAssembly);
            
            await initializer.InitializeAsync(assemblies);
        }

        public async Task InitializeAsync(IEnumerable<Assembly> moduleScanningAssemblies)
        {
            // Migrations are handled by CLI usually, but here we can ensure database is created if not exists.
            // Requirement says: "Tables not found... tables are being created... Varsa... Migration kontrol et"
            // This logic is mostly in CLI flow, but DbInitializer can be a helper.
            // Let's assume database is migrated/created before calling this Seeder.

            // 1. Roles & Ghost Root
            await SeedRolesAndRootAsync();

            // 2. System Settings
            await SeedSystemSettingsAsync();

            // 3. Modules & Permissions
            await SeedModulesAsync(moduleScanningAssemblies);
        }

        private async Task SeedRolesAndRootAsync()
        {
            // 1. Create Roles if they don't exist
            if (!await _context.Roles.AnyAsync(r => r.RoleName == "root"))
            {
                var rootRole = new Role { RoleName = "root" };
                var adminRole = new Role { RoleName = "Admin" };
                var userRole = new Role { RoleName = "User" };

                await _context.Roles.AddRangeAsync(rootRole, adminRole, userRole);
                await _context.SaveChangesAsync();
            }

            // 2. Create Root User if it doesn't exist (separate check!)
            var rootRoleId = await _context.Roles
                .Where(r => r.RoleName == "root")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (rootRoleId > 0 && !await _context.Users.AnyAsync(u => u.RoleId == rootRoleId))
            {
                // Seed Ghost Root User
                // Per requirements: ALL fields (UserName, LastName, Mail, RegNo) must be hashed for root
                var rootUser = new User
                {
                    UserName = "root", // Plaintext
                    UserPassword = _passwordHasher.HashPassword("Q1w2e3r4-"), // Only password hashed
                    UserMail = "root@s2o1.com",
                    UserFirstName = "System",
                    UserLastName = "Root",
                    UserRegNo = "1000",
                    RoleId = rootRoleId,
                    IsActive = true,
                    CreateDate = DateTime.Now
                };
                
                await _context.Users.AddAsync(rootUser);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"[SUCCESS] Root user created with ID: {rootUser.Id}");
            }
        }

        private async Task SeedSystemSettingsAsync()
        {
            if (!await _context.SystemSettings.AnyAsync())
            {
                var settings = new List<SystemSetting>
                {
                    new SystemSetting
                    {
                        SettingKey = "CLI_Welcome_Message",
                        SettingValue = "2S1O - Warehouse Management System [v1.0.0]",
                        AppVersion = "v1.0.0",
                        LogoAscii = @"
 ██████╗ ███████╗ ██╗ ██████╗ 
 ╚════██╗██╔════╝███║██╔═══██╗
  █████╔╝███████╗╚██║██║   ██║
 ██╔═══╝ ╚════██║ ██║██║   ██║
 ███████╗███████║ ██║╚██████╔╝
 ╚══════╝╚══════╝ ╚═╝ ╚═════╝"
                    },
                    new SystemSetting
                    {
                        SettingKey = "BarcodeType",
                        SettingValue = "QR", // Default to QR
                        AppVersion = "v1.0.0"
                    }
                };

                await _context.SystemSettings.AddRangeAsync(settings);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedModulesAsync(IEnumerable<Assembly> assemblies)
        {
            // Scan for Entities (BaseEntity) to define Modules
            var entityNames = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => typeof(S2O1.Domain.Common.BaseEntity).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => t.Name)
                .ToList();

            // Add Static / Special Modules
            var staticModules = new List<string>
            {
                "Reports", "Stock", "Sales", "Warehouse" // Business logical areas
            };

            // Maintain compatibility with existing controller-based naming where it differs
            var legacyModules = new List<string>
            {
                "Users", "Companies", "Invoices", "Offers", "System", "Logs" // Controller-based names
            };

            var allModuleNames = staticModules
                .Union(entityNames)
                .Union(legacyModules)
                .Distinct();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rootRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "root");
                var rootUser = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == rootRole.Id);

                foreach (var moduleName in allModuleNames)
                {
                    var module = await _context.Modules.FirstOrDefaultAsync(m => m.ModuleName == moduleName);
                    if (module == null)
                    {
                        module = new Module { ModuleName = moduleName };
                        await _context.Modules.AddAsync(module);
                        await _context.SaveChangesAsync(); // Save to get Id
                    }
                    if (module == null)
                    {
                        module = new Module { ModuleName = moduleName };
                        await _context.Modules.AddAsync(module);
                        await _context.SaveChangesAsync(); // Save to get Id
                    }

                    // Assign Root Permissions
                    if (rootUser != null)
                    {
                        var perm = await _context.UserPermissions
                            .FirstOrDefaultAsync(p => p.UserId == rootUser.Id && p.ModuleId == module.Id);

                        if (perm == null)
                        {
                            perm = new UserPermission
                            {
                                UserId = rootUser.Id,
                                ModuleId = module.Id,
                                CanRead = true,
                                CanWrite = true,
                                CanDelete = true
                            };
                            await _context.UserPermissions.AddAsync(perm);
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
