using Application.Interfaces;
using Core.Exceptions.JwtExceptions;
using Core.Interfaces;
using Core.Interfaces.DbRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Persistence.Entities;

namespace Application.Handlers.Auth.RefreshToken
{
	public record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResult>;
	public record RefreshTokenResult(string Token, string RefreshToken);
	public class RefreshTokenHandler(IJwtGenerator tokenGenerator, UserManager<UserModel> manager, IUsersRepository usersRepository) : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
	{
		public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
			/*var user = await context.Users.FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken,
				cancellationToken: cancellationToken) ?? throw new InvalidTokenException(request.RefreshToken);

			if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
				throw new InvalidTokenException(request.RefreshToken);

			var userRoles = await manager.GetRolesAsync(user);

			var token = tokenGenerator.CreateToken(user, userRoles);
			var refreshToken = tokenGenerator.CreateRefreshToken();

			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);
			await manager.UpdateAsync(user);

			return new RefreshTokenResult(token, refreshToken);*/
		}
	}
}
