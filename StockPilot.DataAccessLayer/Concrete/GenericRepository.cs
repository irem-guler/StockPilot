using Microsoft.EntityFrameworkCore;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.DataAccessLayer.Context;

namespace StockPilot.DataAccessLayer.Concrete
{
    public class GenericRepository<T> : IGenericDal<T> where T : class
    {
        protected readonly StockPilotContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(StockPilotContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}