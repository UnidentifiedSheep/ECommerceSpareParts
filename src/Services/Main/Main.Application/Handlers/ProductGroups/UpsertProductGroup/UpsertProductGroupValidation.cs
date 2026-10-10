using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Entities.Product;

namespace Main.Application.Handlers.ProductGroups.UpsertProductGroup;

public class UpsertProductGroupValidation : AbstractValidator<UpsertProductGroupCommand>
{
	public UpsertProductGroupValidation()
	{
		RuleFor(command => command.Name)
			.Cascade(CascadeMode.Stop)
			.NotEmpty()
			.WithLocalizableError(ProductGroupNameRequiredMessage.Instance)
			.Must(name => name.Trim().Length >= 3)
			.WithLocalizableError(ProductGroupNameMinLengthMessage.Instance)
			.Must(name => name.Trim().Length <= 256)
			.WithLocalizableError(ProductGroupNameMaxLengthMessage.Instance)
			.Must(name => !string.IsNullOrWhiteSpace(ProductGroup.NormalizeName(name)))
			.WithLocalizableError(ProductGroupNormalizedNameRequiredMessage.Instance)
			.Must(name => ProductGroup.NormalizeName(name).Length <= 256)
			.WithLocalizableError(ProductGroupNormalizedNameMaxLengthMessage.Instance);
	}
}
