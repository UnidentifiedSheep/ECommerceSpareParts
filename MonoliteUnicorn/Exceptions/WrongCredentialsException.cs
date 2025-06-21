using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions
{
	public class WrongCredentialsException : BadRequestException
	{
		public WrongCredentialsException(string details) : base("Неверный логин или пароль", details)
		{
		}
	}
}
