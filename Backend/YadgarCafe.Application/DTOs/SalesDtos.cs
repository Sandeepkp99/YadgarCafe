using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.DTOs
{
    // ---- Customer ----
    public record CustomerRequest(string Name, string PhoneNumber, string? Address);

    public record CustomerResponse(
        Guid Id,
        string Name,
        string PhoneNumber,
        string? Address,
        DateTime CreatedOn,
        DateTime? ModifiedOn);

    // ---- Order ----
    public record OrderItemRequest(Guid ProductId, int Quantity);

    /// <summary>
    /// CustomerId is optional: customers always order for themselves; staff/admin must supply it.
    /// </summary>
    public record CreateOrderRequest(Guid? CustomerId, IReadOnlyList<OrderItemRequest> Items);

    public record UpdateOrderStatusRequest(OrderStatus Status);

    public record UpdatePaymentStatusRequest(PaymentStatus Status);

    public record OrderItemResponse(
        Guid Id,
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice);

    public record OrderResponse(
        Guid Id,
        Guid CustomerId,
        string CustomerName,
        decimal TotalAmount,
        OrderStatus Status,
        PaymentStatus PaymentStatus,
        IReadOnlyList<OrderItemResponse> Items,
        DateTime CreatedOn,
        DateTime? ModifiedOn);

    // ---- Dashboard ----
    public record DashboardStatisticsResponse(
        decimal TodaysSales,
        decimal MonthlySales,
        int CompletedOrders,
        int LowStockCount,
        DateTime GeneratedAtUtc);
}
