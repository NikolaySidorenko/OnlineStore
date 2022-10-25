using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace OnlineStore.Data.Providers.Services.Jwt
{
    public interface IJwtService
    {
        string BuildAccessToken(List<Claim> claims);

        (string Token, DateTime ValidFrom, DateTime Expires) BuildRefreshToken();

        ClaimsPrincipal ValidateToken(string token, TokenValidationParameters parameters);
    }
}
