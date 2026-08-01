using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YadgarCafe.Domain.Common;

namespace YadgarCafe.Domain.Entities
{
    public class Ingredient : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal CurrentStock { get; set; }

        public decimal MinimumStock { get; set; }

        public ICollection<ProductRecipe> ProductRecipes { get; set; }
            = new List<ProductRecipe>();
    }
}
