using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles
{
	public class ArticleNotFoundException : NotFoundException
	{
		public ArticleNotFoundException(object key) : base("Не удалось найти артикул", key)
		{
		}
		public ArticleNotFoundException(IEnumerable<int> ids) : base($"Не удалось найти артикулы ({string.Join(',', ids)})")
		{
		}
		public ArticleNotFoundException() : base($"Не удалось найти какой-то артикул")
		{
		}
	}
}
