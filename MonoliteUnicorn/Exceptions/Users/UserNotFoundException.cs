using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Users
{
	internal class UserNotFoundException : NotFoundException
	{
		public UserNotFoundException() : base("User not found")
		{
			
		}
		public UserNotFoundException(string key) : base($"Не удалось найти пользователя {key}")
		{
		}
	}
}
