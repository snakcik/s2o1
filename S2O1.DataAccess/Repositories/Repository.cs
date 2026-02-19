using Microsoft.EntityFrameworkCore;
using S2O1.Domain.Common;
using S2O1.DataAccess.Contexts;
using S2O1.Core.Interfaces; // Updated namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace S2O1.DataAccess.Repositories
{
    // Interface Removed (Moved to Core)

    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly S2O1DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(S2O1DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
            // _context.Entry(entity).State = EntityState.Modified; 
        }

        public void Remove(T entity)
        {
            // Apply Soft Delete logic if needed, but BaseEntity has IsDeleted.
            // Requirement: "Yetkisiz veri erişimini engellemek için IsDeleted = false ... içeren Global Sorgu Filtreleri"
            // So Remove should probably set IsDeleted = true.
            entity.IsDeleted = true;
            Update(entity);
        }
        
        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}
