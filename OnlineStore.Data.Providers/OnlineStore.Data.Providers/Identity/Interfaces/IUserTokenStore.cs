using OnlineStore.Data.Providers.Context.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineStore.Data.Providers.Identity.Interfaces
{
    public interface IUserTokenStore<TUser> : IDisposable where TUser : class
    {
        Task<RefreshToken> GetRefreshTokenAsync(TUser user, CancellationToken cancellationToken);
        Task<TUser> GetUserByRefreshTokenAsync(string token, CancellationToken cancellationToken);
        Task DeleteRefreshTokenAsync(TUser user, CancellationToken cancellationToken);
        Task ReplaceRefreshTokenAsync(TUser user, RefreshToken token, CancellationToken cancellationToken);
    }
}
