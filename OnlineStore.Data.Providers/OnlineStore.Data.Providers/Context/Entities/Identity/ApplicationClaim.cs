using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineStore.Data.Providers.Context.Entities.Identity
{
    public class ApplicationClaim : IdentityUserClaim<int>
    {
        public ApplicationUser User { get; set; }
    }
}
