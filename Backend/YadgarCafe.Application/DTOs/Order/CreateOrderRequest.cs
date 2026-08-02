namespace YadgarCafe.Application.DTOs.Order
{
    public class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public ICollection<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }

    public class OrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
