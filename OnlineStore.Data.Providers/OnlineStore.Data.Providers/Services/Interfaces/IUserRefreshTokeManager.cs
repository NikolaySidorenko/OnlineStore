using Microsoft.AspNetCore.Identity;
using OnlineStore.Data.Providers.Context.Entities.Identity;
using System;
using System.Threading.Tasks;

namespace OnlineStore.Data.Providers.Services.Interfaces
{
    public interface IUserRefreshTokeManager<TUser> : IDisposable where TUser : class
    {
        Task<RefreshToken> GetRefreshTokenAsync(TUser user);
        Task<TUser> GetUserByRefreshTokenAsync(string token);
        Task<IdentityResult> DeleteRefreshTokenAsync(TUser user);
        Task<IdentityResult> ReplaceRefreshTokenAsync(TUser user, RefreshToken token);
    }
}
