using Application.Common.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Locan.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.StorageContents.SubtractContent;

public class SubtractStorageContentsValidation : AbstractValidator<SubtractStorageContentsCommand>
{
	public SubtractStorageContentsValidation()
	{
		RuleFor(x => x.Items).NotEmpty().WithLocalizableError(StorageContentItemsRequiredMessage.Instance);

		RuleForEach(x => x.Items)
			.ChildRules(item =>
			{
				item
					.RuleFor(x => x.Count)
					.GreaterThan(0)
					.WithLocalizableError(StorageContentCountGreaterThanZeroMessage.Instance);
			});

		RuleForEach(x => x.Items)
			.Custom((item, context) =>
			{
				switch (item)
				{
					case SubtractStorageContentItem { StorageContentId: <= 0 }:
						context.AddFailure(
							CreateFailure(
								nameof(SubtractStorageContentItem.StorageContentId),
								StorageContentIdGreaterThanZeroMessage.Instance));
						break;
					case SubtractProductFromStorageItem byProduct:
						if (byProduct.ProductId <= 0)
							context.AddFailure(
								CreateFailure(
									nameof(SubtractProductFromStorageItem.ProductId),
									ArticleIdGreaterThanZeroMessage.Instance));

						if (string.IsNullOrWhiteSpace(byProduct.StorageCode))
							context.AddFailure(
								CreateFailure(
									nameof(SubtractProductFromStorageItem.StorageCode),
									StorageNameNotEmptyMessage.Instance));

						break;
				}
			});
	}

	private static ValidationFailure CreateFailure(string propertyName, ILocalizableMessage message)
	{
		return new ValidationFailure(propertyName, "Validation failed")
		{
			ErrorCode = message.MessageKey, CustomState = message
		};
	}
}
