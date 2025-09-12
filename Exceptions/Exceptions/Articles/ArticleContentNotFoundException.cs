using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Articles
{
	public class ArticleContentNotFoundException : NotFoundException
	{
		public ArticleContentNotFoundException(int articleId, int insideArticleId) : base("Содержание артикула не было найдено.", 
			new {MainArticleId = articleId, InsideArticleId = insideArticleId})
		{
		}
	}
}
