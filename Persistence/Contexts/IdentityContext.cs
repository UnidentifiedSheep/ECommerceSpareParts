using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.Entities;

namespace Persistence.Contexts;

public class IdentityContext : IdentityDbContext<UserModel>
{
    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}