using YadgarCafe.Application.DTOs.Auth;

namespace YadgarCafe.Application.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(UserDto user);

        string GenerateRefreshToken();

        string HashToken(string token);
    }

    /// <summary>Wraps ASP.NET Identity so the application layer stays framework-free.</summary>
    public interface IIdentityService
    {
        Task<UserDto> CreateUserAsync(string email, string password, string fullName, string role);

        /// <summary>Verifies credentials and applies lockout. Throws <c>UnauthorizedException</c> on failure.</summary>
        Task<UserDto> ValidateCredentialsAsync(string email, string password);

        Task<UserDto?> FindByIdAsync(Guid userId);

        Task<bool> EmailExistsAsync(string email);
    }
}
