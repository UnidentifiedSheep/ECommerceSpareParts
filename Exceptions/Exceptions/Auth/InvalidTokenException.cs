using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Auth
{
	public class InvalidTokenException : BadRequestException
	{
		public InvalidTokenException(string details) : base("Wrong token.", details)
		{
			
		}
	}
}
