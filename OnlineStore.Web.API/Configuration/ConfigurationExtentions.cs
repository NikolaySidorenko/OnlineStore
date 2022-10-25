using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using OnlineStore.Data.Providers.Context;
using OnlineStore.Data.Providers.Context.Entities.Identity;
using OnlineStore.Data.Providers.Identity;
using OnlineStore.Data.Providers.Services;
using OnlineStore.Data.Providers.Services.Jwt;
using OnlineStore.Web.API.Auth;
using OnlineStore.Web.API.Auth.Authorization;
using OnlineStore.Web.API.Auth.Interfaces;
using System.IO;
using System.Text;

namespace OnlineStore.Web.API.Configuration
{
    public static class ConfigurationExtentions
    {
        public static void ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OnlineStoreContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("OnlineStore"));
            });

            services.AddScoped<IAuthManager, AuthManager>();
            services.AddScoped<IJwtService, JwtService>();

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
            })
            .AddEntityFrameworkStores<OnlineStoreContext>()
            .AddUserStore<ApplicationUserStore>()
            .AddUserManager<ApplicationUserMnager>();

            services.Configure<TokenConfiguration>(configuration.GetSection(typeof(TokenConfiguration).Name));

            var tokenConfig = configuration.GetSection(typeof(TokenConfiguration).Name).Get<TokenConfiguration>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Custom"; //JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Custom"; //JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(tokenConfig.Secret)),
                    ValidIssuer = tokenConfig.Issuer,
                    ValidAudience = tokenConfig.Audience,
                    ValidateIssuer = tokenConfig.ValidateIssuer,
                    ValidateAudience = tokenConfig.ValidateAudience,
                    ValidateLifetime = tokenConfig.ValidateLifetime,
                };
            })
            .AddScheme<CustomAuthOptions, CustomAuthHandler>("Custom", options =>
            {
                options.Secret = tokenConfig.Secret;
                options.Issuer = tokenConfig.Issuer;
                options.Audience = tokenConfig.Audience;
                options.ValidateIssuer = tokenConfig.ValidateIssuer;
                options.ValidateAudience = tokenConfig.ValidateAudience;
                options.ValidateLifetime = tokenConfig.ValidateLifetime;
            });


            services.AddSingleton<IAuthorizationHandler, IpAddressRequirementHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IpAddress", p => p.Requirements.Add(new IpAddressRequirement("192")));
                options.AddPolicy("PhoneNumber", p => p.RequireClaim("PhoneNumber"));
                options.AddPolicy("Author", p => p.RequireRole("Author"));
            });
        }

        public static void ConfigureStaticFiles(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            var provider = new FileExtensionContentTypeProvider();

            provider.Mappings[".html"] = "text/html";
            provider.Mappings[".png"] = "image/png";

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "StaticFiles")),
                RequestPath = "/StaticFiles",
                ContentTypeProvider = provider,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append(
                         "Cache-Control", $"public, max-age=604800");
                }
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "StaticFiles")),
                RequestPath = "/StaticFiles"
            });

            //app.UseFileServer(enableDirectoryBrowsing: true);
        }
    }
}
