using OnlineStore.Web.API.Auth.Interfaces;
using OnlineStore.Web.API.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OnlineStore.Data.Providers.Context.Entities.Identity;
using OnlineStore.Data.Providers.Services;
using OnlineStore.Data.Providers.Services.Jwt;
using System.Security.Claims;

namespace OnlineStore.Web.API.Auth
{
    public class AuthManager : IAuthManager
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationUserMnager _userMnager;
        private readonly IJwtService _jwtService;

        public AuthManager(SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, ApplicationUserMnager userMnager, IJwtService jwtService)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userMnager = userMnager;
            _jwtService = jwtService;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string userName, string password)
        {
            if (userName is null || password is null)
            {
                return new AuthenticationResult(AuthResult.Fail);
            }

            var user = await _userMnager.FindByNameAsync(userName);

            if (user is null)
            {
                return new AuthenticationResult(AuthResult.Fail);
            }

            if (!await _userMnager.CheckPasswordAsync(user, password))
            {
                return new AuthenticationResult(AuthResult.Fail);
            }

            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            var accessToken = _jwtService.BuildAccessToken(principal.Claims.ToList());
            var refreshTokenResult = _jwtService.BuildRefreshToken();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenResult.Token,
                ExpiresAt = refreshTokenResult.Expires,
                IssuedAt = refreshTokenResult.ValidFrom,
                UserId = user.Id,
            };

            var result = await _userMnager.ReplaceRefreshTokenAsync(user, refreshToken);

            if (result.Succeeded)
            {
                return new AuthenticationResult(AuthResult.Success, accessToken, refreshTokenResult.Token);
            }

            return new AuthenticationResult(AuthResult.Fail);
        }

        public async Task<AuthenticationResult> RefreshAsync(string token)
        {
            if (token is null)
            {
                return new AuthenticationResult(AuthResult.Fail);
            }

            var user = await _userMnager.GetUserByRefreshTokenAsync(token);

            if (user is null)
            {
                return new AuthenticationResult(AuthResult.Fail);
            }

            var existingToken = await _userMnager.GetRefreshTokenAsync(user);

            if (existingToken.IsExpired)
            {
                return new AuthenticationResult(AuthResult.Fail);
            }

            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            var accessToken = _jwtService.BuildAccessToken(principal.Claims.ToList());
            var refreshTokenResult = _jwtService.BuildRefreshToken();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenResult.Token,
                ExpiresAt = refreshTokenResult.Expires,
                IssuedAt = refreshTokenResult.ValidFrom,
                UserId = user.Id,
            };

            var result = await _userMnager.ReplaceRefreshTokenAsync(user, refreshToken);

            return result.Succeeded ? new AuthenticationResult(AuthResult.Success, accessToken, refreshToken.Token) : new AuthenticationResult(AuthResult.Fail);
        }

        public async Task<AuthenticationResult> RegisterAsync(RegisterModel model)
        {
            if (model is null)
            {
                return new AuthenticationResult(AuthResult.Fail);
            }

            var existingUser = await _userMnager.FindByNameAsync(model.UserName);

            if (existingUser != null)
            {
                return new AuthenticationResult(AuthResult.Fail);
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
            };

            user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, model.Password);

            var role = await _roleManager.FindByNameAsync("User");

            user.Roles.Add(role);

            var result = await _userMnager.CreateAsync(user);

            if (result.Succeeded)
            {
                var newUser = await _userMnager.FindByNameAsync(user.UserName);

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Email, newUser.Email),
                    new Claim(ClaimTypes.MobilePhone, newUser.PhoneNumber),
                    new Claim(ClaimTypes.Name, newUser.UserName),
                    new Claim(ClaimTypes.Role, string.Join(',', user.Roles))
                };

                await _userMnager.AddClaimsAsync(user, claims);

                return new AuthenticationResult(AuthResult.Success);
            }

            return new AuthenticationResult(AuthResult.Fail);
        }
    }
}
