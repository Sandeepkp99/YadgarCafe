using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Common;
using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        private readonly ICurrentUser? _currentUser;

        // currentUser is optional so the design-time factory (migrations) can build the context.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser? currentUser = null)
            : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<ProductRecipe> ProductRecipes => Set<ProductRecipe>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Staff> Staffs => Set<Staff>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Orders keep their customer even if it is later soft deleted; the filter interaction is intended.
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Soft delete: every BaseEntity is hidden once IsDeleted is set, so repositories never repeat the check.
            foreach (var entityType in builder.Model.GetEntityTypes()
                         .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType) && t.ClrType != typeof(ProductRecipe)))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var notDeleted = Expression.Equal(
                    Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                    Expression.Constant(false));

                builder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(notDeleted, parameter));
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditAndSoftDelete();

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("The data was changed by another request. Please retry.");
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                throw new ConflictException("A record with the same unique value already exists.");
            }
        }

        private void ApplyAuditAndSoftDelete()
        {
            var now = DateTime.UtcNow;
            var actor = _currentUser?.UserId?.ToString() ?? "System";

            foreach (var entry in ChangeTracker.Entries<BaseEntity>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = now;
                        entry.Entity.CreatedBy = actor;
                        break;

                    case EntityState.Modified:
                        entry.Property(e => e.CreatedOn).IsModified = false;
                        entry.Property(e => e.CreatedBy).IsModified = false;
                        entry.Entity.ModifiedOn = now;
                        entry.Entity.ModifiedBy = actor;
                        break;

                    case EntityState.Deleted when entry.Entity is not IHardDelete:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.ModifiedOn = now;
                        entry.Entity.ModifiedBy = actor;
                        break;
                }
            }
        }
    }
}
