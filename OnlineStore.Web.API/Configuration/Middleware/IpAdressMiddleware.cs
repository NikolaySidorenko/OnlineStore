using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.Configuration.Middleware
{
    public class IpAdressMiddleware
    {
        private readonly RequestDelegate _next;

        public IpAdressMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress;

            if (ip.ToString().StartsWith("192"))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Your ip in the black list");
            }
            else
            {
                await _next(context);
            }
        }
    }
}
