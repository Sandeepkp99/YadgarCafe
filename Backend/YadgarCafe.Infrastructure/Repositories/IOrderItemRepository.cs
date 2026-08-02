using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Repositories
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetByOrderAsync(Guid orderId);
        Task<OrderItem?> GetByIdAsync(Guid id);
        Task<OrderItem> AddAsync(OrderItem orderItem);
        Task AddRangeAsync(IEnumerable<OrderItem> orderItems);
        Task<bool> DeleteByOrderAsync(Guid orderId);
        Task<bool> ProductExistsAsync(Guid productId);
    }
}
