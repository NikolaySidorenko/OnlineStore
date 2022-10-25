using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineStore.Data.Providers.Context.Entities.Identity;
using OnlineStore.Data.Providers.Identity.Interfaces;
using OnlineStore.Data.Providers.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineStore.Data.Providers.Services
{
    public class ApplicationUserMnager : UserManager<ApplicationUser>, IUserRefreshTokeManager<ApplicationUser>
    {
        public ApplicationUserMnager(
            IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<ApplicationUser> passwordHasher, 
            IEnumerable<IUserValidator<ApplicationUser>> userValidators, 
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger) 
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public async Task<IdentityResult> DeleteRefreshTokenAsync(ApplicationUser user)
        {
            ThrowIfDisposed();

            var tokenStore = GetTokenStore();

            await tokenStore.DeleteRefreshTokenAsync(user, CancellationToken);
            return await UpdateUserAsync(user);
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(ApplicationUser user)
        {
            ThrowIfDisposed();
            
            var tokenStore = GetTokenStore();

            return await tokenStore.GetRefreshTokenAsync(user, CancellationToken);
        }

        public async Task<ApplicationUser> GetUserByRefreshTokenAsync(string token)
        {
            ThrowIfDisposed();

            var tokenStore = GetTokenStore();

            return await tokenStore.GetUserByRefreshTokenAsync(token, CancellationToken);
        }

        public async Task<IdentityResult> ReplaceRefreshTokenAsync(ApplicationUser user, RefreshToken token)
        {
            ThrowIfDisposed();

            var tokenStore = GetTokenStore();

            await tokenStore.ReplaceRefreshTokenAsync(user, token ,CancellationToken);
            return await UpdateUserAsync(user);
        }

        private IUserTokenStore<ApplicationUser> GetTokenStore()
        {
            var store = Store as IUserTokenStore<ApplicationUser>;

            return store ?? throw new InvalidCastException();
        }
    }
}
