using System.Security.Claims;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Common.Constants;

namespace YadgarCafe.API.Services
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

        private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        public Guid? UserId =>
            Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

        public bool IsStaffOrAdmin => Role is Roles.Admin or Roles.Staff;
    }
}
