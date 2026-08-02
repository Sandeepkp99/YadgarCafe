using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using YadgarCafe.Application.Services.Interfaces;

namespace YadgarCafe.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpirationMinutes;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSecret = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret key not configured");
            _jwtIssuer = configuration["Jwt:Issuer"] ?? "YadgarCafe";
            _jwtAudience = configuration["Jwt:Audience"] ?? "YadgarCafeUsers";
            _jwtExpirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "15");
        }

        public string GenerateAccessToken(Guid userId, string email, string userType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("sub", userId.ToString()),
                    new System.Security.Claims.Claim("email", email),
                    new System.Security.Claims.Claim("role", userType),
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public (Guid UserId, string Email, string UserType) ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "sub").Value);
                var email = jwtToken.Claims.First(x => x.Type == "email").Value;
                var userType = jwtToken.Claims.First(x => x.Type == "role").Value;

                return (userId, email, userType);
            }
            catch
            {
                throw new SecurityTokenException("Invalid token");
            }
        }
    }
}
