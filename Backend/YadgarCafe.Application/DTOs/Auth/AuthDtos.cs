namespace YadgarCafe.Application.DTOs.Auth
{
    public record RegisterRequest(string Email, string Password, string ConfirmPassword, string FullName, string? PhoneNumber = null);

    /// <summary>Admin-only: creates an Admin or Staff login.</summary>
    public record CreateStaffUserRequest(string Email, string Password, string FullName, string Role);

    public record LoginRequest(string Email, string Password);

    public record RefreshTokenRequest(string RefreshToken);

    public record UserDto(Guid Id, string Email, string FullName, string Role);

    public record AuthTokenResponse(
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAt,
        UserDto User);
}
