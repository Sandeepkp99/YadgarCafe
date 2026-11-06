using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Common;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Domain.Entities
{
    public class Ingredient : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        /// <summary>Running balance. Changes only through <see cref="ApplyMovement"/>, so every change has a ledger entry.</summary>
        public decimal CurrentStock { get; private set; }

        public decimal MinimumStock { get; set; }

        /// <summary>Optimistic concurrency token, stops two orders from consuming the same stock.</summary>
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public bool IsLowStock => CurrentStock <= MinimumStock;

        public ICollection<ProductRecipe> ProductRecipes { get; set; }
            = new List<ProductRecipe>();

        /// <summary>
        /// Applies a signed stock change and returns the ledger entry recording it.
        /// The caller adds the returned entry to the context.
        /// </summary>
        public Inventory ApplyMovement(StockMovementType type, decimal delta, string? remarks, Guid? orderId = null)
        {
            if (CurrentStock + delta < 0)
                throw new ConflictException(
                    $"Insufficient stock for '{Name}'. Available: {CurrentStock} {Unit}, required: {-delta} {Unit}.");

            CurrentStock += delta;

            return new Inventory
            {
                IngredientId = Id,
                Type = type,
                Quantity = delta,
                BalanceAfter = CurrentStock,
                StockDate = DateTime.UtcNow,
                Remarks = remarks ?? string.Empty,
                OrderId = orderId
            };
        }
    }
}
