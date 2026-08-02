using YadgarCafe.Application.DTOs.Customer;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.DTOs.Order
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public CustomerResponse? Customer { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public IEnumerable<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
