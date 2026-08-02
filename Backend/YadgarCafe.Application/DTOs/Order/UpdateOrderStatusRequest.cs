using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.DTOs.Order
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }

    public class UpdatePaymentStatusRequest
    {
        public PaymentStatus Status { get; set; }
    }
}
