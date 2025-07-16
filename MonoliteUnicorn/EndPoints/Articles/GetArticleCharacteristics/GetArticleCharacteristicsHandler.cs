using Core.Interface;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Anonymous.Articles;
using MonoliteUnicorn.Extensions;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleCharacteristics
{
	public record GetArticleCharacteristicsQuery(int ArticleId) : IQuery<GetArticleCharacteristicsResult>;
	public record GetArticleCharacteristicsResult(IEnumerable<CharacteristicsDto> Characteristics);
	public class GetArticleCharacteristicsValidation : AbstractValidator<GetArticleCharacteristicsQuery>
	{
		public GetArticleCharacteristicsValidation()
		{
			RuleFor(x => x.ArticleId).NotEmpty().WithMessage("Id артикула не должен быть пустым");
		}
	}
	public class GetArticleCharacteristicsHandler(DContext context) : IQueryHandler<GetArticleCharacteristicsQuery, GetArticleCharacteristicsResult>
	{
		public async Task<GetArticleCharacteristicsResult> Handle(GetArticleCharacteristicsQuery request, CancellationToken cancellationToken)
		{
			var character = await context.ArticleCharacteristics
				.AsNoTracking()
				.Where(x => x.ArticleId == request.ArticleId)
				.ToListAsync(cancellationToken);
			return new GetArticleCharacteristicsResult(character.Adapt<List<CharacteristicsDto>>());
		}
	}
}
