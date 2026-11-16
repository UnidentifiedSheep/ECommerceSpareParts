using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Storage;
using Main.Modules.Storage.Abstractions.Models;

namespace Main.Modules.Storage.Application.Services.Storage;

public sealed class
	SubtractStorageContentItemsValidation : AbstractValidator<IReadOnlyCollection<SubtractStorageContentItem>>
{
	public SubtractStorageContentItemsValidation()
	{
		RuleFor(items => items)
			.NotEmpty()
			.WithLocalizableError(StorageContentItemsRequiredMessage.Instance);

		RuleForEach(items => items)
			.ChildRules(item =>
			{
				item.RuleFor(value => value.Count)
					.GreaterThan(0)
					.WithLocalizableError(StorageContentCountGreaterThanZeroMessage.Instance);

				item.RuleFor(value => value.StorageContentId)
					.GreaterThan(0)
					.WithLocalizableError(StorageContentIdGreaterThanZeroMessage.Instance);
			});
	}
}
