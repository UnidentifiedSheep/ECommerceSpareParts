using Core.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Anonymous.Articles;
using MonoliteUnicorn.Extensions;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleContent
{
	public record GetArticleContentQuery(int ArticleId) : IQuery<GetArticleContentResult>;
	public record GetArticleContentResult(IEnumerable<ContentArticleDto> Content);
	public class GetArticleContentHandler(DContext context) : IQueryHandler<GetArticleContentQuery, GetArticleContentResult>
	{
		public async Task<GetArticleContentResult> Handle(GetArticleContentQuery request, CancellationToken cancellationToken)
		{
			var content = await context.ArticlesContents
				.AsNoTracking()
				.Where(x => x.MainArticleId == request.ArticleId)
				.Include(x => x.InsideArticle)
				.ThenInclude(x => x.Producer)
				.ToListAsync(cancellationToken);
			var result = content.Adapt<List<ContentArticleDto>>();
			return new GetArticleContentResult(result);
		}
	}
}
