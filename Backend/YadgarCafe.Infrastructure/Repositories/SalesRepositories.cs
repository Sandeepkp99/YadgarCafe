using Microsoft.EntityFrameworkCore;
using YadgarCafe.Application.Common;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _db;

        public CustomerRepository(ApplicationDbContext db) => _db = db;

        public Task<PagedResult<Customer>> GetPagedAsync(PageQuery page, string? search, CancellationToken ct = default)
        {
            var query = _db.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{QueryExtensions.EscapeLike(search.Trim())}%";
                query = query.Where(c => EF.Functions.Like(c.Name, pattern) || EF.Functions.Like(c.PhoneNumber, pattern));
            }

            return query.OrderBy(c => c.Name).ToPagedAsync(page, ct);
        }

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

        public Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            _db.Customers.FirstOrDefaultAsync(c => c.UserId == userId, ct);

        public Task<Customer?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default) =>
            _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, ct);

        public Task<bool> PhoneExistsAsync(string phoneNumber, Guid? excludeId = null, CancellationToken ct = default) =>
            _db.Customers.AnyAsync(c => c.PhoneNumber == phoneNumber && c.Id != excludeId, ct);

        public Task<bool> HasOrdersAsync(Guid id, CancellationToken ct = default) =>
            _db.Orders.AnyAsync(o => o.CustomerId == id, ct);

        public async Task AddAsync(Customer customer, CancellationToken ct = default) =>
            await _db.Customers.AddAsync(customer, ct);

        public void Remove(Customer customer) => _db.Customers.Remove(customer);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderRepository(ApplicationDbContext db) => _db = db;

        public Task<PagedResult<Order>> GetPagedAsync(
            PageQuery page, Guid? customerId, OrderStatus? status, CancellationToken ct = default)
        {
            var query = _db.Orders.AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsSplitQuery()
                .AsQueryable();

            if (customerId is not null)
                query = query.Where(o => o.CustomerId == customerId);

            if (status is not null)
                query = query.Where(o => o.Status == status);

            return query.OrderByDescending(o => o.CreatedOn).ToPagedAsync(page, ct);
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

        public async Task AddAsync(Order order, CancellationToken ct = default) =>
            await _db.Orders.AddAsync(order, ct);

        public async Task<decimal> GetPaidSalesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default) =>
            await _db.Orders
                .Where(o => o.Status == OrderStatus.Completed
                            && o.PaymentStatus == PaymentStatus.Paid
                            && o.CreatedOn >= fromUtc && o.CreatedOn < toUtc)
                .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;

        public Task<int> CountCompletedAsync(CancellationToken ct = default) =>
            _db.Orders.CountAsync(o => o.Status == OrderStatus.Completed, ct);
    }

    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _db;

        public RefreshTokenRepository(ApplicationDbContext db) => _db = db;

        public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
            _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default) =>
            await _db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync(ct);

        public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
            await _db.RefreshTokens.AddAsync(token, ct);
    }
}
