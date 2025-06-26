using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Users
{
	public class UserNotFoundException : NotFoundException
	{
		public UserNotFoundException() : base("User not found")
		{
			
		}
		public UserNotFoundException(string key) : base($"Не удалось найти пользователя {key}")
		{
		}
	}
}
