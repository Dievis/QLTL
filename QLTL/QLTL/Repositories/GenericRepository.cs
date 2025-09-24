using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace QLTL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            int pageIndex = 1,
            int pageSize = 10)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            int totalCount = await query.CountAsync();

            if (orderBy == null)
                throw new ArgumentException("Phải truyền vào OrderBy khi phân trang!");

            query = orderBy(query)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize);

            var items = await query.ToListAsync();
            return (items, totalCount);
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
