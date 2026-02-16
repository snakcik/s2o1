using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using S2O1.DataAccess.Contexts;
using S2O1.DataAccess.Repositories;
using S2O1.DataAccess.Persistence;
using S2O1.Core.Interfaces; // Added

namespace S2O1.DataAccess
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<S2O1DbContext>(options =>
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(typeof(S2O1DbContext).Assembly.FullName)));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient<DbInitializer>();

            return services;
        }
    }
}
