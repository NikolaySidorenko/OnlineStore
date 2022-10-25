using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OnlineStore.Web.API.Auth.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.Configuration.Middleware
{
    public class HeaderMiddleware : IMiddleware
    {
        private readonly IOptions<HeaderConfiguration> _options;
        private readonly IAuthManager _authManager;

        public HeaderMiddleware(IOptions<HeaderConfiguration> options, IAuthManager authManager)
        {
            _options = options;
            _authManager = authManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var result = _options.Value.RequireUserAgent ? 
                context.Request.Headers.Keys.Any(header => header == "User-Agent") : true;

            if (!result)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Your ip in the black list");
            }
            else
            {
                await next(context);
            }
        }
    }
}
