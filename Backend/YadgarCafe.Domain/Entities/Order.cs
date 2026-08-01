using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YadgarCafe.Domain.Common;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Domain.Entities
{
    public class Order : AuditableEntity
    {
        public Guid CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}
