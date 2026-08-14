using StockPilot.BusinessLayer.Abstract;
using StockPilot.DataAccessLayer.Abstract;
using StockPilot.EntityLayer.Entities;

namespace StockPilot.BusinessLayer.Concrete
{
    public class CustomerManager : ICustomerService
    {
        private readonly ICustomerDal _customerDal;

        public CustomerManager(ICustomerDal customerDal)
        {
            _customerDal = customerDal;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _customerDal.GetAllAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _customerDal.GetByIdAsync(id);
        }

        public async Task AddAsync(Customer customer)
        {
            await _customerDal.AddAsync(customer);
            await _customerDal.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            _customerDal.Update(customer);
            await _customerDal.SaveChangesAsync();
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var customer = await _customerDal.GetByIdAsync(id);

            if (customer == null)
            {
                return false;
            }

            customer.IsActive = false;

            _customerDal.Update(customer);
            await _customerDal.SaveChangesAsync();

            return true;
        }
    }
}