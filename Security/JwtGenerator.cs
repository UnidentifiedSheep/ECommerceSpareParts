using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Core.Dtos.Internal;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Security
{
	public class JwtGenerator(IConfiguration configuration) : IJwtGenerator
	{
		public string CreateToken(UserDto user, IEnumerable<string> roles)
		{
			var handler = new JwtSecurityTokenHandler();
			var privateKey = Encoding.UTF8.GetBytes(configuration["JwtBearer:IssuerSigningKey"]!);
			var credentials = new SigningCredentials(
			 new SymmetricSecurityKey(privateKey),
			 SecurityAlgorithms.HmacSha256);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				SigningCredentials = credentials,
				Issuer = configuration["JwtBearer:ValidIssuer"],
				Expires = DateTime.UtcNow.AddMinutes(60),
				Subject = GetClaims(user, roles)
			};
			return handler.WriteToken(handler.CreateToken(tokenDescriptor));
		}
		private ClaimsIdentity GetClaims(UserDto user, IEnumerable<string> roles)
		{
			var claims = new ClaimsIdentity();
			claims.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? ""));
			claims.AddClaim(new Claim(ClaimTypes.GivenName, user.Name + " " + user.Surname));
			claims.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? ""));
			claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
			foreach (var role in roles)
				claims.AddClaim(new Claim(ClaimTypes.Role, role));
			

			return claims;
		}
		public string CreateRefreshToken()
		{
			var randomNum = new byte[64];
			using var numGenerator = RandomNumberGenerator.Create();
			numGenerator.GetBytes(randomNum);

			return Convert.ToBase64String(randomNum);
		}
		
		public ClaimsPrincipal GetClaimsPrincipal(string token)
		{
			var validation = new TokenValidationParameters
			{
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = configuration["JwtBearer:ValidIssuer"],
				ValidAudience = configuration["JwtBearer:ValidAudience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtBearer:IssuerSigningKey"]!))
			};
			return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
		}
	}
}
