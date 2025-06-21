using Core.Interface;
using Microsoft.AspNetCore.Identity;
using MonoliteUnicorn.Exceptions;
using MonoliteUnicorn.PostGres.Identity;
using MonoliteUnicorn.Services.JWT;

namespace MonoliteUnicorn.EndPoints.Auth.LoginAccount
{
	public record LoginCommand(string Email, string Password) : ICommand<LoginResult>;
	public record LoginResult(string Token, string RefreshToken);
	public class LoginHandler(IJwtGenerator tokenGenerator, UserManager<UserModel> manager) : ICommandHandler<LoginCommand, LoginResult>
	{
		public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
		{
			var user = await manager.FindByEmailAsync(request.Email) ?? throw new WrongCredentialsException(request.Email);
			var checker = await manager.CheckPasswordAsync(user, request.Password) ? true : throw new WrongCredentialsException(request.Email + request.Password);

			var roles = await manager.GetRolesAsync(user);
			var token = tokenGenerator.CreateToken(user, roles);
			var refreshToken = tokenGenerator.CreateRefreshToken();

			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);
			await manager.UpdateAsync(user);
			
			return new LoginResult(token, refreshToken);
		}
	}
}
