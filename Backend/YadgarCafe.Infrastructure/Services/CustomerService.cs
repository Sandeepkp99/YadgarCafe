using YadgarCafe.Application.DTOs.Customer;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Repositories;

namespace YadgarCafe.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;

        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync()
        {
            var customers = await _repository.GetAllAsync();
            return customers.Select(MapToResponse);
        }

        public async Task<CustomerResponse?> GetCustomerByIdAsync(Guid id)
        {
            var customer = await _repository.GetByIdAsync(id);
            return customer != null ? MapToResponse(customer) : null;
        }

        public async Task<CustomerResponse?> GetCustomerByPhoneAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number is required.");

            var customer = await _repository.GetByPhoneNumberAsync(phoneNumber);
            return customer != null ? MapToResponse(customer) : null;
        }

        public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Customer name is required.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new ArgumentException("Phone number is required.");

            if (await _repository.ExistsByPhoneNumberAsync(request.PhoneNumber))
                throw new InvalidOperationException($"A customer with phone number '{request.PhoneNumber}' already exists.");

            var customer = new Customer
            {
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(customer);
            return MapToResponse(created);
        }

        public async Task<CustomerResponse?> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Customer name is required.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new ArgumentException("Phone number is required.");

            var existingCustomer = await _repository.GetByIdAsync(id);
            if (existingCustomer == null)
                return null;

            if (existingCustomer.PhoneNumber != request.PhoneNumber && 
                await _repository.ExistsByPhoneNumberAsync(request.PhoneNumber))
                throw new InvalidOperationException($"A customer with phone number '{request.PhoneNumber}' already exists.");

            var customer = new Customer
            {
                Id = id,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                ModifiedBy = "System"
            };

            var updated = await _repository.UpdateAsync(customer);
            return updated != null ? MapToResponse(updated) : null;
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static CustomerResponse MapToResponse(Customer customer)
        {
            return new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                CreatedOn = customer.CreatedOn,
                ModifiedOn = customer.ModifiedOn
            };
        }
    }
}
