using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles
{
	public class ArticleContentNotFoundException : NotFoundException
	{
		public ArticleContentNotFoundException(object id) : base("Содержание артикула не было найдено.", new {Id = id})
		{
		}
	}
}
