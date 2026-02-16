using Microsoft.EntityFrameworkCore.Storage;
using S2O1.DataAccess.Contexts;
using S2O1.Core.Interfaces; // Updated namespace
using System;
using System.Threading.Tasks;

namespace S2O1.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly S2O1DbContext _context;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(S2O1DbContext context)
        {
            _context = context;
        }

        public IRepository<T> Repository<T>() where T : S2O1.Domain.Common.BaseEntity
        {
            return new Repository<T>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<ITransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return null; 
            }
            _currentTransaction = await _context.Database.BeginTransactionAsync();
            return new EfTransaction(_currentTransaction);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
    
    public class EfTransaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;
        public EfTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }
        public async Task CommitAsync() => await _transaction.CommitAsync();
        public async Task RollbackAsync() => await _transaction.RollbackAsync();
        public void Dispose() => _transaction.Dispose();
    }
}

