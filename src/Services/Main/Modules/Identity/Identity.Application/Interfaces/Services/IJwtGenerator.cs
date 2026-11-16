using System.Security.Claims;
using Main.Modules.Identity.Application.Dtos.Users;

namespace Main.Modules.Identity.Application.Interfaces.Services;

public interface IJwtGenerator
{
	string CreateToken(
		UserDto user,
		IEnumerable<string> roles,
		IEnumerable<string> permissions,
		TimeSpan? additionalValidDuration = null);

	string CreateRefreshToken();

	ClaimsPrincipal GetClaimsPrincipal(string token);
}
