using Application.Common.Extensions;
using FluentValidation;
using Main.Application.Models.Storage;
using Main.Entities;

namespace Main.Application.Services.Storage;

public sealed class SubtractProductFromStorageItemsValidation : AbstractValidator<IReadOnlyCollection<SubtractProductFromStorageItem>>
{
	public SubtractProductFromStorageItemsValidation()
	{
		RuleFor(items => items).NotEmpty().WithLocalizableError(StorageContentItemsRequiredMessage.Instance);
		RuleForEach(items => items).ChildRules(item =>
		{
			item.RuleFor(value => value.Count).GreaterThan(0).WithLocalizableError(StorageContentCountGreaterThanZeroMessage.Instance);
			item.RuleFor(value => value.ProductId).GreaterThan(0).WithLocalizableError(ArticleIdGreaterThanZeroMessage.Instance);
			item.RuleFor(value => value.StorageCode).NotEmpty().WithLocalizableError(StorageNameNotEmptyMessage.Instance);
		});
	}
}
