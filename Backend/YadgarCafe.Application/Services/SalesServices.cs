using Microsoft.Extensions.Options;
using YadgarCafe.Application.Common;
using YadgarCafe.Application.DTOs;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Application.Mappings;
using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customers;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _uow;

        public CustomerService(ICustomerRepository customers, ICurrentUser currentUser, IUnitOfWork uow)
        {
            _customers = customers;
            _currentUser = currentUser;
            _uow = uow;
        }

        public async Task<PagedResult<CustomerResponse>> GetAllAsync(
            PageQuery page, string? search, CancellationToken ct = default) =>
            (await _customers.GetPagedAsync(page, search, ct)).Map(c => c.ToResponse());

        public async Task<CustomerResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var customer = await Find(id, ct);
            EnsureCanAccess(customer);
            return customer.ToResponse();
        }

        public async Task<CustomerResponse> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default) =>
            (await _customers.GetByPhoneAsync(phoneNumber.Trim(), ct)
                ?? throw new NotFoundException("No customer found with this phone number.")).ToResponse();

        public async Task<CustomerResponse> GetMyProfileAsync(CancellationToken ct = default)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedException("Not authenticated.");
            return (await _customers.GetByUserIdAsync(userId, ct)
                ?? throw new NotFoundException("No customer profile is linked to this account.")).ToResponse();
        }

        public async Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken ct = default)
        {
            var phone = request.PhoneNumber.Trim();
            if (await _customers.PhoneExistsAsync(phone, ct: ct))
                throw new ConflictException("A customer with this phone number already exists.");

            var customer = new Customer { Name = request.Name.Trim(), PhoneNumber = phone, Address = request.Address?.Trim() };
            await _customers.AddAsync(customer, ct);
            await _uow.SaveChangesAsync(ct);
            return customer.ToResponse();
        }

        public async Task<CustomerResponse> UpdateAsync(Guid id, CustomerRequest request, CancellationToken ct = default)
        {
            var customer = await Find(id, ct);
            EnsureCanAccess(customer);

            var phone = request.PhoneNumber.Trim();
            if (await _customers.PhoneExistsAsync(phone, id, ct))
                throw new ConflictException("A customer with this phone number already exists.");

            customer.Name = request.Name.Trim();
            customer.PhoneNumber = phone;
            customer.Address = request.Address?.Trim();

            await _uow.SaveChangesAsync(ct);
            return customer.ToResponse();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var customer = await Find(id, ct);

            if (await _customers.HasOrdersAsync(id, ct))
                throw new ConflictException("This customer has orders and cannot be deleted.");

            _customers.Remove(customer);
            await _uow.SaveChangesAsync(ct);
        }

        private async Task<Customer> Find(Guid id, CancellationToken ct) =>
            await _customers.GetByIdAsync(id, ct) ?? throw new NotFoundException("Customer", id);

        /// <summary>Staff/admin can see any customer; a customer only their own profile.</summary>
        private void EnsureCanAccess(Customer customer)
        {
            if (_currentUser.IsStaffOrAdmin) return;
            if (customer.UserId is null || customer.UserId != _currentUser.UserId)
                throw new ForbiddenException();
        }
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orders;
        private readonly ICustomerRepository _customers;
        private readonly IProductRepository _products;
        private readonly IProductRecipeRepository _recipes;
        private readonly IInventoryRepository _inventory;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _uow;

        public OrderService(
            IOrderRepository orders,
            ICustomerRepository customers,
            IProductRepository products,
            IProductRecipeRepository recipes,
            IInventoryRepository inventory,
            ICurrentUser currentUser,
            IUnitOfWork uow)
        {
            _orders = orders;
            _customers = customers;
            _products = products;
            _recipes = recipes;
            _inventory = inventory;
            _currentUser = currentUser;
            _uow = uow;
        }

        public async Task<PagedResult<OrderResponse>> GetAllAsync(
            PageQuery page, Guid? customerId, OrderStatus? status, CancellationToken ct = default)
        {
            // Customers only ever see their own orders, whatever filter they send.
            if (!_currentUser.IsStaffOrAdmin)
                customerId = (await GetOwnCustomerAsync(ct)).Id;

            return (await _orders.GetPagedAsync(page, customerId, status, ct)).Map(o => o.ToResponse());
        }

        public async Task<OrderResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var order = await Find(id, ct);
            await EnsureCanAccessAsync(order, ct);
            return order.ToResponse();
        }

        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
        {
            var customer = await ResolveCustomerAsync(request.CustomerId, ct);

            // Merge repeated lines for the same product so stock is checked against the real total.
            var lines = request.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var products = (await _products.GetByIdsAsync(lines.Keys, ct)).ToDictionary(p => p.Id);

            var order = new Order { CustomerId = customer.Id, Customer = customer };

            foreach (var (productId, quantity) in lines)
            {
                if (!products.TryGetValue(productId, out var product))
                    throw new NotFoundException("Product", productId);

                if (!product.IsAvailable)
                    throw new ConflictException($"Product '{product.Name}' is not available.");

                order.AddItem(product, quantity);
            }

            // One stock movement per ingredient, summed across every ordered product.
            var recipes = await _recipes.GetByProductIdsAsync(lines.Keys, ct);
            var consumption = recipes
                .GroupBy(r => r.Ingredient)
                .Select(g => (Ingredient: g.Key, Quantity: g.Sum(r => r.Quantity * lines[r.ProductId])));

            var ledger = new List<Inventory>();
            foreach (var (ingredient, quantity) in consumption)
            {
                // Throws ConflictException when stock is insufficient, before anything is saved.
                ledger.Add(ingredient.ApplyMovement(
                    StockMovementType.OrderConsumption, -quantity, $"Order {order.Id}", order.Id));
            }

            await _orders.AddAsync(order, ct);
            await _inventory.AddRangeAsync(ledger, ct);

            // Order, items, stock balances and ledger are saved in one transaction;
            // the ingredient RowVersion rejects a concurrent order that consumed the same stock.
            await _uow.SaveChangesAsync(ct);

            return order.ToResponse();
        }

        public async Task<OrderResponse> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct = default)
        {
            var order = await Find(id, ct);

            if (status == OrderStatus.Cancelled)
                return await CancelAsync(id, ct);

            order.ChangeStatus(status);
            await _uow.SaveChangesAsync(ct);
            return order.ToResponse();
        }

        public async Task<OrderResponse> UpdatePaymentStatusAsync(Guid id, PaymentStatus status, CancellationToken ct = default)
        {
            var order = await Find(id, ct);
            order.ChangePaymentStatus(status);
            await _uow.SaveChangesAsync(ct);
            return order.ToResponse();
        }

        public async Task<OrderResponse> CancelAsync(Guid id, CancellationToken ct = default)
        {
            var order = await Find(id, ct);
            await EnsureCanAccessAsync(order, ct);

            if (!_currentUser.IsStaffOrAdmin && order.Status != OrderStatus.Pending)
                throw new ConflictException("You can only cancel an order while it is still pending.");

            order.Cancel();

            // Give back exactly what this order consumed.
            var ledger = new List<Inventory>();
            foreach (var consumed in await _inventory.GetOrderConsumptionAsync(order.Id, ct))
            {
                ledger.Add(consumed.Ingredient.ApplyMovement(
                    StockMovementType.OrderRestore, -consumed.Quantity, $"Order {order.Id} cancelled", order.Id));
            }

            await _inventory.AddRangeAsync(ledger, ct);
            await _uow.SaveChangesAsync(ct);
            return order.ToResponse();
        }

        private async Task<Order> Find(Guid id, CancellationToken ct) =>
            await _orders.GetByIdAsync(id, ct) ?? throw new NotFoundException("Order", id);

        private async Task<Customer> GetOwnCustomerAsync(CancellationToken ct)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedException("Not authenticated.");
            return await _customers.GetByUserIdAsync(userId, ct)
                ?? throw new NotFoundException("No customer profile is linked to this account.");
        }

        private async Task<Customer> ResolveCustomerAsync(Guid? requestedId, CancellationToken ct)
        {
            if (!_currentUser.IsStaffOrAdmin)
            {
                var own = await GetOwnCustomerAsync(ct);
                if (requestedId is not null && requestedId != own.Id)
                    throw new ForbiddenException("You can only place orders for yourself.");
                return own;
            }

            if (requestedId is null || requestedId == Guid.Empty)
                throw new BadRequestException("CustomerId is required when placing an order on behalf of a customer.");

            return await _customers.GetByIdAsync(requestedId.Value, ct)
                ?? throw new NotFoundException("Customer", requestedId.Value);
        }

        private async Task EnsureCanAccessAsync(Order order, CancellationToken ct)
        {
            if (_currentUser.IsStaffOrAdmin) return;

            var own = await GetOwnCustomerAsync(ct);
            if (order.CustomerId != own.Id)
                throw new ForbiddenException();
        }
    }

    public class DashboardService : IDashboardService
    {
        private readonly IOrderRepository _orders;
        private readonly IIngredientRepository _ingredients;
        private readonly TimeZoneInfo _zone;

        public DashboardService(
            IOrderRepository orders,
            IIngredientRepository ingredients,
            IOptions<BusinessOptions> options)
        {
            _orders = orders;
            _ingredients = ingredients;
            _zone = ResolveZone(options.Value.TimeZoneId);
        }

        public async Task<DashboardStatisticsResponse> GetStatisticsAsync(CancellationToken ct = default)
        {
            var nowUtc = DateTime.UtcNow;
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _zone);

            // "Today" and "this month" are business-local, converted back to UTC for the query.
            var startOfDay = TimeZoneInfo.ConvertTimeToUtc(localNow.Date, _zone);
            var startOfMonth = TimeZoneInfo.ConvertTimeToUtc(new DateTime(localNow.Year, localNow.Month, 1), _zone);

            // The same scoped DbContext backs every repository, so these run one after another.
            var today = await _orders.GetPaidSalesAsync(startOfDay, nowUtc.AddSeconds(1), ct);
            var month = await _orders.GetPaidSalesAsync(startOfMonth, nowUtc.AddSeconds(1), ct);
            var completed = await _orders.CountCompletedAsync(ct);
            var lowStock = await _ingredients.CountLowStockAsync(ct);

            return new DashboardStatisticsResponse(today, month, completed, lowStock, nowUtc);
        }

        private static TimeZoneInfo ResolveZone(string id)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
            catch (InvalidTimeZoneException) { return TimeZoneInfo.Utc; }
        }
    }
}
