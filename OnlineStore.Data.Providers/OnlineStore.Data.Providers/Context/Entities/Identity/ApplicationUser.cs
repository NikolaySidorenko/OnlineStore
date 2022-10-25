using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace OnlineStore.Data.Providers.Context.Entities.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public List<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
        public List<ApplicationClaim> Claims { get; set; } = new List<ApplicationClaim>();
        public int RefreshTokenId { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}
