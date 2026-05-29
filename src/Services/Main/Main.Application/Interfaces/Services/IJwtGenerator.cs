using System.Security.Claims;
using Main.Application.Dtos.Users;

namespace Main.Application.Interfaces.Services;

public interface IJwtGenerator
{
    string CreateToken(
        UserDto user,
        string deviceId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        TimeSpan? additionalValidDuration = null);

    string CreateRefreshToken();
    ClaimsPrincipal GetClaimsPrincipal(string token);
}