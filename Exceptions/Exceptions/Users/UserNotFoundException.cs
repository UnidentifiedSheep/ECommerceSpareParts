using Exceptions.Base;

namespace Exceptions.Exceptions.Users
{
	public class UserNotFoundException : NotFoundException
	{
		public UserNotFoundException() : base("User not found")
		{
			
		}
		public UserNotFoundException(string id) : base($"Не удалось найти пользователя", new { Id = id })
		{
		}
		
		public UserNotFoundException(IEnumerable<string> ids) : base($"Не удалось найти пользователя", new { Ids = ids })
		{
		}
	}
}
