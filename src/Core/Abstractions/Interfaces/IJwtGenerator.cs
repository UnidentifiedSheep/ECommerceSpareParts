using System.Security.Claims;
using Abstractions.Models;

namespace Abstractions.Interfaces;

public interface IJwtGenerator
{
    string CreateToken(User user, UserInfo userInfo, string deviceId, IEnumerable<string> roles, 
        IEnumerable<string> permissions);
    string CreateRefreshToken();
    ClaimsPrincipal GetClaimsPrincipal(string token);
}