using YadgarCafe.Application.DTOs.Customer;

namespace YadgarCafe.Application.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync();
        Task<CustomerResponse?> GetCustomerByIdAsync(Guid id);
        Task<CustomerResponse?> GetCustomerByPhoneAsync(string phoneNumber);
        Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
        Task<CustomerResponse?> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request);
        Task<bool> DeleteCustomerAsync(Guid id);
    }
}
