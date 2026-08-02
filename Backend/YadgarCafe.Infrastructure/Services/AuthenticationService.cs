using Microsoft.AspNetCore.Identity;
using YadgarCafe.Application.DTOs.Auth;
using YadgarCafe.Application.Services.Interfaces;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Infrastructure.Persistence;

namespace YadgarCafe.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Email) || 
                    string.IsNullOrWhiteSpace(request.Password) || 
                    string.IsNullOrWhiteSpace(request.FullName))
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Email, password, and full name are required." 
                    };
                }

                if (request.Password != request.ConfirmPassword)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Passwords do not match." 
                    };
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "User with this email already exists." 
                    };
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    UserType = Domain.Enums.UserType.Customer, // Default to Customer
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = $"Registration failed: {errors}" 
                    };
                }

                // Assign default role
                await _userManager.AddToRoleAsync(user, "Customer");

                return new AuthResponse
                {
                    Success = true,
                    Message = "User registered successfully."
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"An error occurred during registration: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || 
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Email and password are required." 
                    };
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Invalid email or password." 
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Invalid email or password." 
                    };
                }

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, user.UserType.ToString());
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    Expires = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };
                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    Data = new AuthTokenResponse
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresIn = DateTime.UtcNow.AddMinutes(15),
                        User = new UserDto
                        {
                            Id = user.Id,
                            Email = user.Email!,
                            FullName = user.FullName,
                            UserType = user.UserType.ToString()
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"An error occurred during login: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Refresh token is required." 
                    };
                }

                var storedToken = _context.RefreshTokens
                    .FirstOrDefault(x => x.Token == request.RefreshToken && !x.IsRevoked);

                if (storedToken == null || storedToken.Expires < DateTime.UtcNow)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "Invalid or expired refresh token." 
                    };
                }

                var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
                if (user == null)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "User not found." 
                    };
                }

                // Generate new tokens
                var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, user.UserType.ToString());
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Revoke old refresh token and save new one
                storedToken.IsRevoked = true;
                _context.RefreshTokens.Update(storedToken);

                var newRefreshTokenEntity = new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    Expires = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };
                _context.RefreshTokens.Add(newRefreshTokenEntity);
                await _context.SaveChangesAsync();

                return new AuthResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully.",
                    Data = new AuthTokenResponse
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,
                        ExpiresIn = DateTime.UtcNow.AddMinutes(15),
                        User = new UserDto
                        {
                            Id = user.Id,
                            Email = user.Email!,
                            FullName = user.FullName,
                            UserType = user.UserType.ToString()
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"An error occurred during token refresh: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> LogoutAsync(Guid userId)
        {
            try
            {
                // Revoke all refresh tokens for this user
                var userTokens = _context.RefreshTokens
                    .Where(x => x.UserId == userId && !x.IsRevoked)
                    .ToList();

                foreach (var token in userTokens)
                {
                    token.IsRevoked = true;
                }

                _context.RefreshTokens.UpdateRange(userTokens);
                await _context.SaveChangesAsync();

                return new AuthResponse
                {
                    Success = true,
                    Message = "Logout successful."
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"An error occurred during logout: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        Message = "User not found." 
                    };
                }

                return new AuthResponse
                {
                    Success = true,
                    Message = "User retrieved successfully.",
                    Data = new AuthTokenResponse
                    {
                        AccessToken = string.Empty,
                        RefreshToken = string.Empty,
                        ExpiresIn = DateTime.UtcNow,
                        User = new UserDto
                        {
                            Id = user.Id,
                            Email = user.Email!,
                            FullName = user.FullName,
                            UserType = user.UserType.ToString()
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
