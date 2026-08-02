using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(Guid id);
        Task<Customer?> GetByPhoneNumberAsync(string phoneNumber);
        Task<Customer> AddAsync(Customer customer);
        Task<Customer?> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
    }
}
