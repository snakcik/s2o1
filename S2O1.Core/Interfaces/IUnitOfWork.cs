using S2O1.Domain.Common;
using System;
using System.Threading.Tasks;

namespace S2O1.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync();
        // IDbContextTransaction is EF Core specific.
        // To keep Business clean of EF Core, we should wrap Transaction or use a transparent mechanism.
        // But for strict "No EF constraints", using IDbContextTransaction directly violates it if referenced.
        // However, IDbContextTransaction is in Microsoft.EntityFrameworkCore.Storage.
        // Standard "Core" interfaces should return IDisposable transaction wrapper.
        Task<ITransaction> BeginTransactionAsync();
        IRepository<T> Repository<T>() where T : BaseEntity;
    }
    
    public interface ITransaction : IDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
