using Exceptions.Base;

namespace Exceptions.Exceptions.Articles
{
	public class ArticleContentNotFoundException : NotFoundException
	{
		public ArticleContentNotFoundException(int articleId, int insideArticleId) : base("Содержание артикула не было найдено.", 
			new {MainArticleId = articleId, InsideArticleId = insideArticleId})
		{
		}
	}
}
