using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Infrastructure.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByCustomerAsync(Guid customerId);
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order> AddAsync(Order order);
        Task<Order?> UpdateAsync(Order order);
        Task<bool> DeleteAsync(Guid id);
        Task<decimal> GetTodaysSalesAsync();
        Task<decimal> GetMonthlySalesAsync();
        Task<int> GetTotalOrdersAsync();
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<bool> CustomerExistsAsync(Guid customerId);
    }
}
