using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlineStore.Web.API.Auth.Models;

namespace OnlineStore.Web.API.Auth.Interfaces
{
    public interface IAuthManager
    {
        Task<AuthenticationResult> RegisterAsync(RegisterModel model);
        Task<AuthenticationResult> AuthenticateAsync(string userName, string password);
        Task<AuthenticationResult> RefreshAsync(string token);
    }
}
