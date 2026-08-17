using StockPilot.EntityLayer.Entities;

namespace StockPilot.DataAccessLayer.Abstract
{
    public interface IPurchaseOrderDal : IGenericDal<PurchaseOrder>
    {
        Task<List<PurchaseOrder>> GetAllWithDetailsAsync();

        Task<PurchaseOrder?> GetByIdWithDetailsAsync(int id);
    }
}