using Microsoft.AspNetCore.Identity;

namespace Persistence.Entities
{
	public class UserModel : IdentityUser
	{
		public string Name { get; set; } = null!;
		public string Surname { get; set; } = null!;
		public string? RefreshToken { get; set; }
		public bool IsSupplier { get; set; } = false;
		public DateTime? RefreshTokenExpiryTime { get; set; }
		public string? Description { get; set; }
	}
}
