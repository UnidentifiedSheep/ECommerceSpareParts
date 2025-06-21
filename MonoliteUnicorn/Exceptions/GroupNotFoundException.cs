using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions
{
	public class GroupNotFoundException : NotFoundException
	{
		public GroupNotFoundException(object key) : base("Группа не найдена!", key)
		{
		}
	}
}
