using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Articles
{
	public class ArticleContentNotFoundException : NotFoundException
	{
		public ArticleContentNotFoundException(object key) : base("Содержание артикула не было найдено.", key)
		{
		}
	}
}
