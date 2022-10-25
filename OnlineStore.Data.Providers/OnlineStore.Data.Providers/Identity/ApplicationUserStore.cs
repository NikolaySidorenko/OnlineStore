using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineStore.Data.Providers.Context;
using OnlineStore.Data.Providers.Context.Entities.Identity;
using OnlineStore.Data.Providers.Identity.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineStore.Data.Providers.Identity
{
    public class ApplicationUserStore : IApplicationUserStore
    {
        private bool _disposed = false;

        protected ILogger<ApplicationUserStore> Logger;
        protected OnlineStoreContext Context;
        protected HashSet<int> _loadedUsers = new HashSet<int>();

        public ApplicationUserStore(OnlineStoreContext context, ILogger<ApplicationUserStore> logger)
        {
            Context = context;
            Logger = logger;
        }

        public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);
            ThrowIfNull(claims);

            var appClaims = claims.Select(x => new ApplicationClaim
            {
                ClaimType = x.Type,
                ClaimValue = x.Value,
            });

            foreach (var claim in appClaims)
            {
                user.Claims.Add(claim);
            }

            await Context.SaveChangesAsync();
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            Context.Users.Add(user);

            await Context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            Context.Users.Remove(user);

            await Context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task DeleteRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            var token = await Context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
            Context.RefreshTokens.Remove(token);
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(userId);

            if (int.TryParse(userId, out int id))
            {
                var user = Context.Users.Find(id);
                if (user == null)
                {
                    throw new ArgumentException($"User with id = {userId} doesn't exist");
                }

                await LoadUserData(user, cancellationToken);
                return user;
            }
            else
            {
                throw new ArgumentException(nameof(userId));
            }
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(normalizedUserName);

            var user = await Context.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName);
            await LoadUserData(user, cancellationToken);

            return user;
        }

        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            await LoadUserData(user, cancellationToken);

            return Context.UserClaims.Where(c => c.UserId == user.Id).Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            return Task.FromResult(user.PasswordHash);
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            return await Context.RefreshTokens.Where(x => x.UserId == user.Id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ApplicationUser> GetUserByRefreshTokenAsync(string token, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(token);

            return await Context.Users.Include(u => u.RefreshToken).FirstOrDefaultAsync(u => u.RefreshToken.Token == token);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            return Task.FromResult(user.UserName);
        }

        public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(claim);

            return await Context.Users
                .Where(u => u.Claims.Any(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value))
                .ToListAsync();
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            return Task.FromResult(user.PasswordHash != null);
        }

        public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);
            ThrowIfNull(claims);

            foreach(var claim in claims)
            {
                var userClaims = await Context.UserClaims
                    .Where(c => c.UserId == user.Id && c.ClaimType == claim.Type && c.ClaimValue == claim.Value)
                    .ToListAsync(cancellationToken);

                Context.UserClaims.RemoveRange(userClaims);

                await Context.SaveChangesAsync();
            }
        }

        public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);
            ThrowIfNull(claim);
            ThrowIfNull(newClaim);

            var matchedClaims = await Context.UserClaims
                .Where(u => u.UserId == user.Id && u.ClaimValue == claim.Value && u.ClaimType == claim.Type)
                .ToListAsync(cancellationToken);

            foreach (var matchedClaim in matchedClaims)
            {
                matchedClaim.ClaimType = newClaim.Type;
                matchedClaim.ClaimValue = newClaim.Value;
            }

            await Context.SaveChangesAsync();
        }

        public async Task ReplaceRefreshTokenAsync(ApplicationUser user, RefreshToken token, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);
            ThrowIfNull(token);

            var exitingToken = await Context.RefreshTokens
                .Where(x => x.UserId == user.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (exitingToken != null)
            {
                Context.RefreshTokens.Remove(exitingToken);
            }

            Context.RefreshTokens.Add(token);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            user.NormalizedUserName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            user.PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            user.UserName = userName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfNull(user);

            Context.Update(user);

            await Context.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_disposed)
            {
                return;
            }

            if (isDisposing)
            {
                Context?.Dispose();
            }

            _disposed = true;
            Context = null;
        }

        protected virtual void ThrowIfNull<TArgument>(TArgument argument) where TArgument : class
        {
            if(argument is null)
            {
                throw new ArgumentNullException(argument.GetType().Name);
            }
        }

        private async Task LoadUserData(ApplicationUser user, CancellationToken cancellationToken)
        {
            if (user != null && !_loadedUsers.Contains(user.Id))
            {
                var refName = nameof(user.RefreshToken);
                await Context.Entry(user).Collection(u => u.Claims).LoadAsync(cancellationToken);
                await Context.Entry(user).Collection(u => u.Roles).LoadAsync(cancellationToken);
                await Context.Entry(user).Navigation(refName).LoadAsync(cancellationToken);

                _loadedUsers.Add(user.Id);
            }
        }
    }
}
