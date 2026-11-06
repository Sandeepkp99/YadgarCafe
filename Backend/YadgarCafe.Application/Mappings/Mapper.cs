using YadgarCafe.Application.DTOs;
using YadgarCafe.Application.DTOs.Auth;
using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Application.Mappings
{
    /// <summary>Entity to DTO projections. Kept explicit so no mapping surprises hide in a library.</summary>
    public static class Mapper
    {
        public static CategoryResponse ToResponse(this Category c) =>
            new(c.Id, c.Name, c.CreatedOn, c.ModifiedOn);

        public static ProductResponse ToResponse(this Product p) =>
            new(p.Id, p.Name, p.Price, p.IsAvailable, p.CategoryId, p.Category?.Name ?? string.Empty,
                p.CreatedOn, p.ModifiedOn);

        public static IngredientResponse ToResponse(this Ingredient i) =>
            new(i.Id, i.Name, i.Unit, i.CurrentStock, i.MinimumStock, i.IsLowStock, i.CreatedOn, i.ModifiedOn);

        public static InventoryResponse ToResponse(this Inventory i) =>
            new(i.Id, i.IngredientId, i.Ingredient?.Name ?? string.Empty, i.Ingredient?.Unit ?? string.Empty,
                i.Type, i.Quantity, i.BalanceAfter, i.OrderId, i.StockDate, i.Remarks, i.CreatedBy);

        public static RecipeResponse ToResponse(this ProductRecipe r) =>
            new(r.Id, r.ProductId, r.Product?.Name ?? string.Empty, r.IngredientId,
                r.Ingredient?.Name ?? string.Empty, r.Ingredient?.Unit ?? string.Empty, r.Quantity,
                r.Ingredient?.CurrentStock ?? 0, (r.Ingredient?.CurrentStock ?? 0) >= r.Quantity);

        public static CustomerResponse ToResponse(this Customer c) =>
            new(c.Id, c.Name, c.PhoneNumber, c.Address, c.CreatedOn, c.ModifiedOn);

        public static OrderResponse ToResponse(this Order o) =>
            new(o.Id, o.CustomerId, o.Customer?.Name ?? string.Empty, o.TotalAmount, o.Status, o.PaymentStatus,
                o.OrderItems.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Quantity,
                    i.UnitPrice, i.TotalPrice)).ToList(),
                o.CreatedOn, o.ModifiedOn);
    }
}
