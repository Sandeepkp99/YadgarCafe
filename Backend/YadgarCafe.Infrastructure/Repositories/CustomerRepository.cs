using Microsoft.EntityFrameworkCore;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && !c.IsDeleted);
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            customer.CreatedOn = DateTime.UtcNow;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer?> UpdateAsync(Customer customer)
        {
            var existingCustomer = await GetByIdAsync(customer.Id);
            if (existingCustomer == null)
                return null;

            existingCustomer.Name = customer.Name;
            existingCustomer.PhoneNumber = customer.PhoneNumber;
            existingCustomer.Address = customer.Address;
            existingCustomer.ModifiedOn = DateTime.UtcNow;

            _context.Customers.Update(existingCustomer);
            await _context.SaveChangesAsync();

            return existingCustomer;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var customer = await GetByIdAsync(id);
            if (customer == null)
                return false;

            customer.IsDeleted = true;
            customer.ModifiedOn = DateTime.UtcNow;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Customers
                .AnyAsync(c => c.PhoneNumber == phoneNumber && !c.IsDeleted);
        }
    }
}
