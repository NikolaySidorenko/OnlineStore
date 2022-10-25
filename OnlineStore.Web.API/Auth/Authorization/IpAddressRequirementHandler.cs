using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.Auth.Authorization
{
    public class IpAddressRequirementHandler : AuthorizationHandler<IpAddressRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IpAddressRequirement requirement)
        {
            if(!context.User.Claims.Any(x => x.Type == "IpAddress" && !x.Value.StartsWith(requirement.IpAddress)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
