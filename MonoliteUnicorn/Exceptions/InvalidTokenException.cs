using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions
{
	public class InvalidTokenException : BadRequestException
	{
		public InvalidTokenException(string details) : base("Wrong token.", details)
		{
			
		}
	}
}
