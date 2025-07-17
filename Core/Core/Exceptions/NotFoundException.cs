namespace Core.Exceptions
{
	public class NotFoundException : BaseValuedException
	{
		public NotFoundException(string message) : base(message) { }
		public NotFoundException(string message, object relatedData) : base(message, relatedData) { }
	}
}
