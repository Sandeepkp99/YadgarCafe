using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YadgarCafe.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        Preparing = 2,
        Ready = 3,
        Completed = 4,
        Cancelled = 5
    }
}
