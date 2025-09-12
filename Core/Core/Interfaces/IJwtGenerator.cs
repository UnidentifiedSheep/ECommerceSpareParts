using System.Security.Claims;
using Core.Dtos.Internal;

namespace Core.Interfaces;

public interface IJwtGenerator
{
    string CreateToken(UserDto user, IEnumerable<string> roles);
    string CreateRefreshToken();
    ClaimsPrincipal GetClaimsPrincipal(string token);
}