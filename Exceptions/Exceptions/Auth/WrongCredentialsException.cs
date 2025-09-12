using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Auth
{
	public class WrongCredentialsException : BadRequestException
	{
		public WrongCredentialsException(string details) : base("Неверный логин или пароль", details)
		{
		}
	}
}
