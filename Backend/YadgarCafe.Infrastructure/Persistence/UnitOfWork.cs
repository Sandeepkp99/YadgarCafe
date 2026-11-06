using Microsoft.EntityFrameworkCore;
using YadgarCafe.Application.Interfaces;

namespace YadgarCafe.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            _context.SaveChangesAsync(ct);

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
        {
            // Already inside a transaction: join it instead of nesting.
            if (_context.Database.CurrentTransaction is not null)
                return await action();

            // The retrying execution strategy only allows transactions that it runs itself,
            // so the whole unit is wrapped and replayed as one piece if a transient fault happens.
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);
                var result = await action();
                await transaction.CommitAsync(ct);
                return result;
            });
        }
    }
}
