using YadgarCafe.Application.DTOs.Auth;

namespace YadgarCafe.Application.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<AuthResponse> LogoutAsync(Guid userId);
        Task<AuthResponse> GetCurrentUserAsync(Guid userId);
    }
}
