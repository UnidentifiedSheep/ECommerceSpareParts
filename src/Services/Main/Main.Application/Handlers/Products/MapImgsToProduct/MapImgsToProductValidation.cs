using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Products.MapImgsToProduct;

public class MapImgsToProductValidation : AbstractValidator<MapImgsToProductCommand>
{
	public static readonly string[] ImageExtensions =
	[
		".png",
		".jpeg",
		".jpg",
		".gif",
		".bmp",
		".webp",
		".tiff"
	];

	public MapImgsToProductValidation()
	{
		RuleForEach(x => x.Images)
			.ChildRules(z =>
			{
				z
					.RuleFor(x => x.Extension)
					.Must(x => ImageExtensions.Any(c => c == x))
					.WithLocalizableError(ArticleImageInvalidExtensionMessage.Instance);
			});

		RuleFor(x => x.Images).NotEmpty().WithLocalizableError(ArticleImagesMustNotBeEmptyMessage.Instance);
	}
}
