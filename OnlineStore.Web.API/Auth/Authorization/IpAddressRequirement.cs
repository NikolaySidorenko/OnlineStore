using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.Auth.Authorization
{
    public class IpAddressRequirement : IAuthorizationRequirement
    {
        public string IpAddress { get; set; }

        public IpAddressRequirement(string ipAddress)
        {
            IpAddress = ipAddress;
        }
    }
}
