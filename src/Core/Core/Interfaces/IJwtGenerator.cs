using System.Security.Claims;
using Core.Models;

namespace Core.Interfaces;

public interface IJwtGenerator
{
    string CreateToken(User user, UserInfo userInfo, string deviceId, IEnumerable<string> roles, 
        IEnumerable<string> permissions);
    string CreateRefreshToken();
    ClaimsPrincipal GetClaimsPrincipal(string token);
}