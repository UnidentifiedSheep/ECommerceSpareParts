using System.Security.Claims;
using Core.Entities;

namespace Core.Interfaces;

public interface IJwtGenerator
{
    string CreateToken(User user, UserInfo userInfo, string deviceId, IEnumerable<string> roles);
    string CreateRefreshToken();
    ClaimsPrincipal GetClaimsPrincipal(string token);
}