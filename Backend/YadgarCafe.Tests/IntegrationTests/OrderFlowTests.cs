using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YadgarCafe.Application.Common;
using YadgarCafe.Application.DTOs;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Application.Services;
using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;
using YadgarCafe.Infrastructure.Persistence;
using YadgarCafe.Infrastructure.Repositories;

namespace YadgarCafe.Tests.IntegrationTests
{
    /// <summary>
    /// Runs the real services and repositories against an in-memory SQLite database,
    /// so soft delete filters, tracking and transactions behave like production.
    /// </summary>
    public sealed class OrderFlowTests : IDisposable
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");
        private readonly TestUser _user = new() { Role = "Admin", UserId = Guid.NewGuid() };
        private readonly ApplicationDbContext _db;

        public OrderFlowTests()
        {
            _connection.Open();
            _db = Create();
            _db.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        private ApplicationDbContext Create() =>
            new SqliteTestContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options, _user);

        /// <summary>SQLite cannot generate SQL Server's rowversion, so tests store it as a plain value.</summary>
        private sealed class SqliteTestContext : ApplicationDbContext
        {
            public SqliteTestContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser user)
                : base(options, user) { }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);
                builder.Entity<Ingredient>().Property(i => i.RowVersion)
                    .ValueGeneratedNever().IsConcurrencyToken();
            }
        }

        private OrderService Orders(ApplicationDbContext db) => new(
            new OrderRepository(db), new CustomerRepository(db), new ProductRepository(db),
            new ProductRecipeRepository(db), new InventoryRepository(db), _user, new UnitOfWork(db));

        private async Task<(Guid productId, Guid customerId, Ingredient milk)> SeedAsync(decimal milkStock, decimal perCup)
        {
            var category = new Category { Name = "Drinks" };
            var product = new Product { Name = "Chai", Price = 20, IsAvailable = true, Category = category };
            var milk = new Ingredient { Name = "Milk", Unit = "L" };
            milk.RowVersion = new byte[] { 1 };
            var customer = new Customer { Name = "Asha", PhoneNumber = "9999999999" };

            _db.AddRange(category, product, milk, customer);
            await _db.SaveChangesAsync();

            var entry = milk.ApplyMovement(StockMovementType.Purchase, milkStock, "seed");
            _db.Inventories.Add(entry);
            _db.ProductRecipes.Add(new ProductRecipe { ProductId = product.Id, IngredientId = milk.Id, Quantity = perCup });
            await _db.SaveChangesAsync();

            return (product.Id, customer.Id, milk);
        }

        private static CreateOrderRequest Order(Guid customerId, Guid productId, int qty) =>
            new(customerId, new[] { new OrderItemRequest(productId, qty) });

        [Fact]
        public async Task Creating_an_order_deducts_stock_and_writes_ledger()
        {
            var (productId, customerId, milk) = await SeedAsync(milkStock: 10, perCup: 0.2m);

            var order = await Orders(_db).CreateAsync(Order(customerId, productId, 5), default);

            Assert.Equal(100, order.TotalAmount);
            Assert.Equal(9m, (await _db.Ingredients.FindAsync(milk.Id))!.CurrentStock);

            var ledger = await _db.Inventories.Where(i => i.OrderId == order.Id).ToListAsync();
            var row = Assert.Single(ledger);
            Assert.Equal(StockMovementType.OrderConsumption, row.Type);
            Assert.Equal(-1m, row.Quantity);
        }

        [Fact]
        public async Task Order_is_rejected_when_stock_is_insufficient_and_nothing_is_saved()
        {
            var (productId, customerId, milk) = await SeedAsync(milkStock: 1, perCup: 0.5m);

            await Assert.ThrowsAsync<ConflictException>(() =>
                Orders(_db).CreateAsync(Order(customerId, productId, 5), default));

            Assert.Empty(_db.Orders);
            Assert.Equal(1m, (await _db.Ingredients.FindAsync(milk.Id))!.CurrentStock);
        }

        [Fact]
        public async Task Repeated_lines_for_one_product_are_checked_against_their_total()
        {
            var (productId, customerId, _) = await SeedAsync(milkStock: 1, perCup: 0.5m);

            var request = new CreateOrderRequest(customerId, new[]
            {
                new OrderItemRequest(productId, 1),
                new OrderItemRequest(productId, 2)
            });

            // 3 cups need 1.5 L, but only 1 L exists; each line alone would have passed.
            await Assert.ThrowsAsync<ConflictException>(() => Orders(_db).CreateAsync(request, default));
        }

        [Fact]
        public async Task Cancelling_restores_exactly_what_was_consumed()
        {
            var (productId, customerId, milk) = await SeedAsync(milkStock: 10, perCup: 0.2m);
            var service = Orders(_db);

            var order = await service.CreateAsync(Order(customerId, productId, 5), default);
            var cancelled = await service.CancelAsync(order.Id, default);

            Assert.Equal(OrderStatus.Cancelled, cancelled.Status);
            Assert.Equal(10m, (await _db.Ingredients.FindAsync(milk.Id))!.CurrentStock);
            Assert.Contains(_db.Inventories, i => i.OrderId == order.Id && i.Type == StockMovementType.OrderRestore);
        }

        [Fact]
        public async Task A_cancelled_order_cannot_be_cancelled_twice()
        {
            var (productId, customerId, milk) = await SeedAsync(milkStock: 10, perCup: 0.2m);
            var service = Orders(_db);

            var order = await service.CreateAsync(Order(customerId, productId, 5), default);
            await service.CancelAsync(order.Id, default);

            await Assert.ThrowsAsync<ConflictException>(() => service.CancelAsync(order.Id, default));
            Assert.Equal(10m, (await _db.Ingredients.FindAsync(milk.Id))!.CurrentStock);
        }

        [Fact]
        public async Task Customer_cannot_view_or_cancel_another_customers_order()
        {
            var (productId, customerId, _) = await SeedAsync(milkStock: 10, perCup: 0.2m);
            var order = await Orders(_db).CreateAsync(Order(customerId, productId, 1), default);

            var stranger = new Customer { Name = "Ravi", PhoneNumber = "8888888888", UserId = Guid.NewGuid() };
            _db.Customers.Add(stranger);
            await _db.SaveChangesAsync();

            _user.Role = "Customer";
            _user.UserId = stranger.UserId;
            var service = Orders(_db);

            await Assert.ThrowsAsync<ForbiddenException>(() => service.GetByIdAsync(order.Id, default));
            await Assert.ThrowsAsync<ForbiddenException>(() => service.CancelAsync(order.Id, default));
        }

        [Fact]
        public async Task Customer_orders_for_themselves_only()
        {
            var (productId, customerId, _) = await SeedAsync(milkStock: 10, perCup: 0.2m);
            var own = new Customer { Name = "Meera", PhoneNumber = "7777777777", UserId = Guid.NewGuid() };
            _db.Customers.Add(own);
            await _db.SaveChangesAsync();

            _user.Role = "Customer";
            _user.UserId = own.UserId;

            var placed = await Orders(_db).CreateAsync(new CreateOrderRequest(null, new[] { new OrderItemRequest(productId, 1) }), default);
            Assert.Equal(own.Id, placed.CustomerId);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                Orders(_db).CreateAsync(Order(customerId, productId, 1), default));
        }

        [Fact]
        public async Task Soft_deleted_products_cannot_be_ordered()
        {
            var (productId, customerId, _) = await SeedAsync(milkStock: 10, perCup: 0.2m);
            await new ProductService(
                new ProductRepository(_db), new CategoryRepository(_db),
                new ProductRecipeRepository(_db), new UnitOfWork(_db)).DeleteAsync(productId);

            Assert.True((await _db.Products.IgnoreQueryFilters().SingleAsync()).IsDeleted);
            await Assert.ThrowsAsync<NotFoundException>(() =>
                Orders(_db).CreateAsync(Order(customerId, productId, 1), default));
        }

        [Fact]
        public async Task Audit_fields_are_filled_from_the_current_user()
        {
            var (_, customerId, _) = await SeedAsync(milkStock: 1, perCup: 0.1m);

            var customer = await _db.Customers.FindAsync(customerId);
            Assert.Equal(_user.UserId.ToString(), customer!.CreatedBy);
            Assert.NotEqual(default, customer.CreatedOn);
        }

        private sealed class TestUser : ICurrentUser
        {
            public Guid? UserId { get; set; }
            public string? Role { get; set; }
            public bool IsAuthenticated => UserId is not null;
            public bool IsStaffOrAdmin => Role is "Admin" or "Staff";
        }
    }
}
