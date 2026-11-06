using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.DTOs
{
    /// <summary>
    /// A manual stock movement. Quantity is always positive; the type decides the direction:
    /// OpeningBalance/Purchase add, Wastage subtracts, Adjustment uses <see cref="IsIncrease"/>.
    /// </summary>
    public record StockMovementRequest(
        Guid IngredientId,
        StockMovementType Type,
        decimal Quantity,
        bool IsIncrease = true,
        string? Remarks = null);

    public record InventoryResponse(
        Guid Id,
        Guid IngredientId,
        string IngredientName,
        string Unit,
        StockMovementType Type,
        decimal Quantity,
        decimal BalanceAfter,
        Guid? OrderId,
        DateTime StockDate,
        string Remarks,
        string CreatedBy);
}
