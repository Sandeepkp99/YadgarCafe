using System.ComponentModel.DataAnnotations;

namespace YadgarCafe.Infrastructure.Identity
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required, MinLength(32)]
        public string SecretKey { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = "YadgarCafe";

        [Required]
        public string Audience { get; set; } = "YadgarCafeUsers";

        [Range(1, 1440)]
        public int ExpirationMinutes { get; set; } = 15;
    }
}
