using Core.Exceptions.JwtExceptions;
using Core.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.PostGres.Identity;
using MonoliteUnicorn.Services.JWT;

namespace MonoliteUnicorn.EndPoints.Auth.RefreshToken
{
	public record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResult>;
	public record RefreshTokenResult(string Token, string RefreshToken);
	public class RefreshTokenHandler(IJwtGenerator tokenGenerator, UserManager<UserModel> manager, IdentityContext context) : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
	{
		public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
		{
			var user = await context.Users.FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken, 
				cancellationToken: cancellationToken) ?? throw new InvalidTokenException(request.RefreshToken);

			if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
				throw new InvalidTokenException(request.RefreshToken);

			var userRoles = await manager.GetRolesAsync(user);

			var token = tokenGenerator.CreateToken(user, userRoles);
			var refreshToken = tokenGenerator.CreateRefreshToken();

			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);
			await manager.UpdateAsync(user);

			return new RefreshTokenResult(token, refreshToken);
		}
	}
}
