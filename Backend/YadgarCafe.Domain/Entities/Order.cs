using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Common;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Domain.Entities
{
    public class Order : BaseEntity
    {
        private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> StatusFlow =
            new Dictionary<OrderStatus, OrderStatus[]>
            {
                [OrderStatus.Pending] = new[] { OrderStatus.Preparing, OrderStatus.Cancelled },
                [OrderStatus.Preparing] = new[] { OrderStatus.Ready, OrderStatus.Cancelled },
                [OrderStatus.Ready] = new[] { OrderStatus.Completed, OrderStatus.Cancelled },
                [OrderStatus.Completed] = Array.Empty<OrderStatus>(),
                [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
            };

        private static readonly IReadOnlyDictionary<PaymentStatus, PaymentStatus[]> PaymentFlow =
            new Dictionary<PaymentStatus, PaymentStatus[]>
            {
                [PaymentStatus.Pending] = new[] { PaymentStatus.Paid, PaymentStatus.Failed },
                [PaymentStatus.Failed] = new[] { PaymentStatus.Paid },
                [PaymentStatus.Paid] = new[] { PaymentStatus.Refunded },
                [PaymentStatus.Refunded] = Array.Empty<PaymentStatus>()
            };

        public Guid CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;

        public decimal TotalAmount { get; private set; }

        public OrderStatus Status { get; private set; } = OrderStatus.Pending;

        public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();

        public void AddItem(Product product, int quantity)
        {
            if (quantity <= 0)
                throw new BadRequestException("Quantity must be greater than zero.");

            OrderItems.Add(new OrderItem
            {
                OrderId = Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * quantity
            });

            TotalAmount = OrderItems.Sum(i => i.TotalPrice);
        }

        public void ChangeStatus(OrderStatus next)
        {
            if (!StatusFlow[Status].Contains(next))
                throw new ConflictException($"An order cannot move from {Status} to {next}.");

            Status = next;
        }

        public void Cancel()
        {
            if (Status is OrderStatus.Completed or OrderStatus.Cancelled)
                throw new ConflictException("Cannot cancel a completed or already cancelled order.");

            Status = OrderStatus.Cancelled;

            if (PaymentStatus == PaymentStatus.Paid)
                PaymentStatus = PaymentStatus.Refunded;
        }

        public void ChangePaymentStatus(PaymentStatus next)
        {
            if (Status == OrderStatus.Cancelled)
                throw new ConflictException("The payment of a cancelled order cannot be changed.");

            if (!PaymentFlow[PaymentStatus].Contains(next))
                throw new ConflictException($"Payment cannot move from {PaymentStatus} to {next}.");

            PaymentStatus = next;
        }
    }
}
