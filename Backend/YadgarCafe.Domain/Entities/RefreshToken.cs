using YadgarCafe.Domain.Common;

namespace YadgarCafe.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        /// <summary>SHA-256 hash of the token. The raw value is only ever seen by the client.</summary>
        public string TokenHash { get; set; } = string.Empty;

        public DateTime Expires { get; set; }

        public bool IsRevoked { get; set; }

        public Guid UserId { get; set; }

        public ApplicationUser User { get; set; } = null!;

        public bool IsActive => !IsRevoked && Expires > DateTime.UtcNow;
    }
}
