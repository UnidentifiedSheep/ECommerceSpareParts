using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MonoliteUnicorn.PostGres.Identity
{
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
}
