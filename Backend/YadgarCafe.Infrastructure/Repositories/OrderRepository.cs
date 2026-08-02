using Microsoft.EntityFrameworkCore;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.CreatedOn)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId && !o.IsDeleted)
                .OrderByDescending(o => o.CreatedOn)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }

        public async Task<Order> AddAsync(Order order)
        {
            order.CreatedOn = DateTime.UtcNow;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> UpdateAsync(Order order)
        {
            var existingOrder = await GetByIdAsync(order.Id);
            if (existingOrder == null)
                return null;

            existingOrder.Status = order.Status;
            existingOrder.PaymentStatus = order.PaymentStatus;
            existingOrder.TotalAmount = order.TotalAmount;
            existingOrder.ModifiedOn = DateTime.UtcNow;

            _context.Orders.Update(existingOrder);
            await _context.SaveChangesAsync();

            return existingOrder;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var order = await GetByIdAsync(id);
            if (order == null)
                return false;

            order.IsDeleted = true;
            order.ModifiedOn = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetTodaysSalesAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Orders
                .Where(o => !o.IsDeleted && 
                       o.Status == OrderStatus.Completed && 
                       o.PaymentStatus == PaymentStatus.Paid &&
                       o.CreatedOn.Date == today)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<decimal> GetMonthlySalesAsync()
        {
            var now = DateTime.UtcNow;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            return await _context.Orders
                .Where(o => !o.IsDeleted && 
                       o.Status == OrderStatus.Completed && 
                       o.PaymentStatus == PaymentStatus.Paid &&
                       o.CreatedOn >= firstDay)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersAsync()
        {
            return await _context.Orders
                .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
                .CountAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.Status == status && !o.IsDeleted)
                .OrderByDescending(o => o.CreatedOn)
                .ToListAsync();
        }

        public async Task<bool> CustomerExistsAsync(Guid customerId)
        {
            return await _context.Customers
                .AnyAsync(c => c.Id == customerId && !c.IsDeleted);
        }
    }
}
