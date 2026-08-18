using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Abstract
{
    public interface ISalesOrderDal : IGenericDal<SalesOrder>
    {
        Task<List<SalesOrder>> GetAllWithDetailsAsync();

        Task<SalesOrder?> GetByIdWithDetailsAsync(int id);
    }
}