using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YadgarCafe.Domain.Common;

namespace YadgarCafe.Domain.Entities
{
    public class Inventory : AuditableEntity
    {
        public Guid IngredientId { get; set; }

        public Ingredient Ingredient { get; set; } = null!;

        public decimal Quantity { get; set; }

        public DateTime StockDate { get; set; }

        public string Remarks { get; set; } = string.Empty;
    }
}
