namespace YadgarCafe.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(Guid userId, string email, string userType);
        string GenerateRefreshToken();
        (Guid UserId, string Email, string UserType) ValidateToken(string token);
    }
}
