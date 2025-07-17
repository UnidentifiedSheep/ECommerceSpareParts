using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles
{
	public class ArticleNotFoundException : NotFoundException
	{
		public ArticleNotFoundException(object id) : base("Не удалось найти артикул.", new {Id = id})
		{
		}
		public ArticleNotFoundException(IEnumerable<int> ids) : base($"Не удалось найти артикулы.", new {Ids = ids})
		{
		}
		public ArticleNotFoundException() : base($"Не удалось найти какой-то артикул")
		{
		}
	}
}
