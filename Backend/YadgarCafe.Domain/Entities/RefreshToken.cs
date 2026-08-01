using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YadgarCafe.Domain.Common;

namespace YadgarCafe.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;

        public DateTime Expires { get; set; }

        public bool IsRevoked { get; set; }

        public Guid UserId { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
