using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace OnlineStore.Data.Providers.Services.Jwt
{
    public class JwtService : IJwtService
    {
        private readonly IOptions<TokenConfiguration> _config;

        public JwtService(IOptions<TokenConfiguration> config)
        {
            _config = config;
        }

        public string BuildAccessToken(List<Claim> claims)
        {
            if (claims is null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            return BuildToken(_config.Value.AccessTokenExpiration, claims).Token;
        }

        public (string Token, DateTime ValidFrom, DateTime Expires) BuildRefreshToken() => BuildToken(_config.Value.RefreshTokenExpiration);

        public (string Token, DateTime ValidFrom, DateTime Expires) BuildToken(int expiration, IEnumerable<Claim> claims = default)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.Value.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken(
                    issuer: _config.Value.Issuer,
                    audience: _config.Value.Audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(expiration),
                    signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), token.ValidFrom, token.ValidTo);
        }

        public ClaimsPrincipal ValidateToken(string token, TokenValidationParameters parameters)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken validatedToken;
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out validatedToken);
            return principal;
        }
    }
}
