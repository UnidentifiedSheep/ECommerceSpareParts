using System.Security.Claims;
using MonoliteUnicorn.PostGres.Identity;

namespace MonoliteUnicorn.Services.JWT
{
	public interface IJwtGenerator
	{
		string CreateToken(UserModel user, IEnumerable<string> roles);
		string CreateRefreshToken();
		ClaimsPrincipal GetClaimsPrincipal(string token);
	}
}