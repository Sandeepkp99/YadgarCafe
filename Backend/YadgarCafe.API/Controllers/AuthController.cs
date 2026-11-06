using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YadgarCafe.Application.DTOs.Auth;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Common.Constants;

namespace YadgarCafe.API.Controllers
{
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>Self-register as a customer.</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<UserDto>> Register(RegisterRequest request, CancellationToken ct) =>
            StatusCode(StatusCodes.Status201Created, await _auth.RegisterAsync(request, ct));

        /// <summary>Create an Admin or Staff login. Admin only.</summary>
        [HttpPost("staff-users")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<UserDto>> CreateStaffUser(CreateStaffUserRequest request, CancellationToken ct) =>
            StatusCode(StatusCodes.Status201Created, await _auth.CreateStaffUserAsync(request, ct));

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthTokenResponse>> Login(LoginRequest request, CancellationToken ct) =>
            Ok(await _auth.LoginAsync(request, ct));

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthTokenResponse>> RefreshToken(RefreshTokenRequest request, CancellationToken ct) =>
            Ok(await _auth.RefreshTokenAsync(request, ct));

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            await _auth.LogoutAsync(CurrentUserId, ct);
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> Me(CancellationToken ct) =>
            Ok(await _auth.GetCurrentUserAsync(CurrentUserId, ct));
    }
}
