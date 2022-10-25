using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.Auth.Models
{
    public class AuthenticationResult
    {
        public bool IsSuccess => _result == AuthResult.Success;

        private AuthResult _result;
        public string AccessToken { get; }
        public string RefreshToken { get; }

        public AuthenticationResult(AuthResult result)
        {
            _result = result;
        }

        public AuthenticationResult(AuthResult result, string accessToken, string refreshToken) : this(result)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
