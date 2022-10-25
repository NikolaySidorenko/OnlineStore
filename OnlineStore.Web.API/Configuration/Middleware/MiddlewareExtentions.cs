using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.Configuration.Middleware
{
    public static class MiddlewareExtentions
    {
        public static IApplicationBuilder UseIpAdresses(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IpAdressMiddleware>();
        }

        public static IApplicationBuilder UseHeader(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HeaderMiddleware>();
        }
    }
}
