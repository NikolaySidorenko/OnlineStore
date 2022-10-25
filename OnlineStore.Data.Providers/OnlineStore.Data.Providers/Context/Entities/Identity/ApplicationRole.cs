using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace OnlineStore.Data.Providers.Context.Entities.Identity
{
    public class ApplicationRole : IdentityRole<int>
    {
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
