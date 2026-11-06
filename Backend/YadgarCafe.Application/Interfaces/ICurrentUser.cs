namespace YadgarCafe.Application.Interfaces
{
    /// <summary>The caller of the current request. Implemented by the API layer.</summary>
    public interface ICurrentUser
    {
        Guid? UserId { get; }

        string? Role { get; }

        bool IsAuthenticated { get; }

        bool IsStaffOrAdmin { get; }
    }
}
