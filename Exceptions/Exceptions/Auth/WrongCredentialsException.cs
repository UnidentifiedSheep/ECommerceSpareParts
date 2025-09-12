using Exceptions.Base;

namespace Exceptions.Exceptions.Auth
{
	public class WrongCredentialsException : BadRequestException
	{
		public WrongCredentialsException(string details) : base("Неверный логин или пароль", details)
		{
		}
	}
}
