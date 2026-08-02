using Microsoft.EntityFrameworkCore;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderAsync(Guid orderId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<OrderItem?> GetByIdAsync(Guid id)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task<OrderItem> AddAsync(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
            return orderItem;
        }

        public async Task AddRangeAsync(IEnumerable<OrderItem> orderItems)
        {
            _context.OrderItems.AddRange(orderItems);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteByOrderAsync(Guid orderId)
        {
            var items = await GetByOrderAsync(orderId);
            if (!items.Any())
                return false;

            _context.OrderItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ProductExistsAsync(Guid productId)
        {
            return await _context.Products
                .AnyAsync(p => p.Id == productId && !p.IsDeleted);
        }
    }
}
