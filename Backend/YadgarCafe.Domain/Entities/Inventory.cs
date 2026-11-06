using YadgarCafe.Domain.Common;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Domain.Entities
{
    /// <summary>Stock ledger entry. Quantity is signed (negative means stock left).</summary>
    public class Inventory : BaseEntity
    {
        public Guid IngredientId { get; set; }

        public Ingredient Ingredient { get; set; } = null!;

        public StockMovementType Type { get; set; }

        public decimal Quantity { get; set; }

        public decimal BalanceAfter { get; set; }

        public Guid? OrderId { get; set; }

        public DateTime StockDate { get; set; }

        public string Remarks { get; set; } = string.Empty;
    }
}
