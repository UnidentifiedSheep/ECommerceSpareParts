using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions
{
	public class EmptyArticleListException : BadRequestException
	{
		public EmptyArticleListException() : base("Список артикулов пуст!")
		{
		}
	}
}
