using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sample.Core.Identity.Data.Enities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Core.Identity.Data.DbContexts
{
    public class ApplicationUserDbContext: IdentityDbContext<ApplicationUser>
    {
        public ApplicationUserDbContext(DbContextOptions<ApplicationUserDbContext> options) : base(options)
        {

        }
    }
}
