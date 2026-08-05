using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPilot.DataAccessLayer.Abstract
{
    public interface IGenericDal<T> where T : class
    {
        Task<List<T>> GetAllAsync();

        Task<T?> GetByIdAsync(int id);

        Task AddAsync(T entity);

        void Update(T entity);

        Task<int> SaveChangesAsync();
    }
}
