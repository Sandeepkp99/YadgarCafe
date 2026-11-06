using YadgarCafe.Application.DTOs.Auth;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Common.Constants;
using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Entities;

namespace YadgarCafe.Application.Services
{
    public class AuthService : IAuthService
    {
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

        private readonly IIdentityService _identity;
        private readonly ITokenService _tokens;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ICustomerRepository _customers;
        private readonly IUnitOfWork _uow;

        public AuthService(
            IIdentityService identity,
            ITokenService tokens,
            IRefreshTokenRepository refreshTokens,
            ICustomerRepository customers,
            IUnitOfWork uow)
        {
            _identity = identity;
            _tokens = tokens;
            _refreshTokens = refreshTokens;
            _customers = customers;
            _uow = uow;
        }

        public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            // Self-registration can only ever create a Customer.
            if (await _identity.EmailExistsAsync(request.Email))
                throw new ConflictException("A user with this email already exists.");

            var phone = request.PhoneNumber?.Trim() ?? string.Empty;
            if (phone.Length > 0 && await _customers.PhoneExistsAsync(phone, ct: ct))
                throw new ConflictException("A customer with this phone number already exists.");

            return await _uow.ExecuteInTransactionAsync(async () =>
            {
                var user = await _identity.CreateUserAsync(
                    request.Email, request.Password, request.FullName, Roles.Customer);

                await _customers.AddAsync(new Customer
                {
                    Name = request.FullName.Trim(),
                    PhoneNumber = phone,
                    UserId = user.Id
                }, ct);
                await _uow.SaveChangesAsync(ct);

                return user;
            }, ct);
        }

        public async Task<UserDto> CreateStaffUserAsync(CreateStaffUserRequest request, CancellationToken ct = default)
        {
            if (await _identity.EmailExistsAsync(request.Email))
                throw new ConflictException("A user with this email already exists.");

            return await _identity.CreateUserAsync(request.Email, request.Password, request.FullName, request.Role);
        }

        public async Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _identity.ValidateCredentialsAsync(request.Email, request.Password);
            return await IssueTokensAsync(user, ct);
        }

        public async Task<AuthTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
        {
            var stored = await _refreshTokens.GetByHashAsync(_tokens.HashToken(request.RefreshToken), ct);
            if (stored is null)
                throw new UnauthorizedException("Invalid refresh token.");

            if (stored.IsRevoked)
            {
                // A rotated token is being replayed: assume theft and end every session of this user.
                await RevokeAllAsync(stored.UserId, ct);
                await _uow.SaveChangesAsync(ct);
                throw new UnauthorizedException("Refresh token has already been used. Please log in again.");
            }

            if (stored.Expires <= DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token has expired.");

            var user = await _identity.FindByIdAsync(stored.UserId)
                ?? throw new UnauthorizedException("User no longer exists.");

            stored.IsRevoked = true;
            return await IssueTokensAsync(user, ct);
        }

        public async Task LogoutAsync(Guid userId, CancellationToken ct = default)
        {
            await RevokeAllAsync(userId, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default) =>
            await _identity.FindByIdAsync(userId) ?? throw new NotFoundException("User", userId);

        private async Task<AuthTokenResponse> IssueTokensAsync(UserDto user, CancellationToken ct)
        {
            var (accessToken, expiresAt) = _tokens.GenerateAccessToken(user);
            var refreshToken = _tokens.GenerateRefreshToken();

            await _refreshTokens.AddAsync(new RefreshToken
            {
                TokenHash = _tokens.HashToken(refreshToken),
                UserId = user.Id,
                Expires = DateTime.UtcNow.Add(RefreshTokenLifetime)
            }, ct);
            await _uow.SaveChangesAsync(ct);

            return new AuthTokenResponse(accessToken, refreshToken, expiresAt, user);
        }

        private async Task RevokeAllAsync(Guid userId, CancellationToken ct)
        {
            foreach (var token in await _refreshTokens.GetActiveByUserAsync(userId, ct))
                token.IsRevoked = true;
        }
    }
}
