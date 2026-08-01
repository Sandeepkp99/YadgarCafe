using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();
    }
}
