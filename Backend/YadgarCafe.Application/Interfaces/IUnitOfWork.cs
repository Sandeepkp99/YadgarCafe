namespace YadgarCafe.Application.Interfaces
{
    public interface IUnitOfWork
    {
        /// <summary>Persists staged changes. Throws <c>ConflictException</c> on concurrent modification.</summary>
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        /// <summary>Runs several saves (including identity operations) as one atomic unit.</summary>
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default);
    }
}
