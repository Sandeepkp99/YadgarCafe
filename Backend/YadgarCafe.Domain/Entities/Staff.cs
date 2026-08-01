using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YadgarCafe.Domain.Common;

namespace YadgarCafe.Domain.Entities
{
    public class Staff : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Designation { get; set; } = string.Empty;
    }
}
