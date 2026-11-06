namespace YadgarCafe.Application.DTOs
{
    // ---- Category ----
    public record CategoryRequest(string Name);

    public record CategoryResponse(Guid Id, string Name, DateTime CreatedOn, DateTime? ModifiedOn);

    // ---- Product ----
    public record ProductRequest(string Name, decimal Price, bool IsAvailable, Guid CategoryId);

    public record ProductResponse(
        Guid Id,
        string Name,
        decimal Price,
        bool IsAvailable,
        Guid CategoryId,
        string CategoryName,
        DateTime CreatedOn,
        DateTime? ModifiedOn);

    // ---- Ingredient ----
    /// <summary>
    /// Stock is not part of this request: it can only change through inventory movements,
    /// so the stock ledger always explains the balance.
    /// </summary>
    public record IngredientRequest(string Name, string Unit, decimal MinimumStock);

    public record IngredientResponse(
        Guid Id,
        string Name,
        string Unit,
        decimal CurrentStock,
        decimal MinimumStock,
        bool IsLowStock,
        DateTime CreatedOn,
        DateTime? ModifiedOn);

    // ---- Recipe ----
    public record CreateRecipeRequest(Guid ProductId, Guid IngredientId, decimal Quantity);

    public record UpdateRecipeRequest(decimal Quantity);

    public record RecipeResponse(
        Guid Id,
        Guid ProductId,
        string ProductName,
        Guid IngredientId,
        string IngredientName,
        string Unit,
        decimal Quantity,
        decimal CurrentStock,
        bool IsAvailable);

    public record ProductRecipeDetailResponse(
        Guid ProductId,
        string ProductName,
        decimal ProductPrice,
        bool CanProduce,
        IReadOnlyList<RecipeResponse> Ingredients);
}
