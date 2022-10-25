using Microsoft.AspNetCore.Identity;
using OnlineStore.Data.Providers.Context.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineStore.Data.Providers.Identity.Interfaces
{
    public interface IApplicationUserStore : 
        IUserStore<ApplicationUser>,
        IUserClaimStore<ApplicationUser>,
        IUserTokenStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>
    {
    }
}
