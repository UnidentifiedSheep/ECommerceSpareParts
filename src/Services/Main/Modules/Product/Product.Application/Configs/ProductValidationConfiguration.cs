using BulkValidation.Core.Configuration;
using Main.Entities.Product;
using Main.Entities.Product.Exceptions;

namespace Main.Modules.Product.Application.Configs;

public static class ProductValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
	{
		errors.For(ProducerAliasKeys.Alias)
			.OnMissing(input => new ProducersAliasNotFoundException(string.Join(", ", input.Values)));

		errors.For(ProducerAliasKeys.Alias)
			.OnAlreadyExists(input => new ProducerAliasAlreadyTakenException(input.Values));

		errors.For(ProductKeys.Id)
			.OnMissing(input => input.IsBatch
				? new ProductNotFoundException(input.Values)
				: new ProductNotFoundException(input.Values[0]));

		errors.For(ProductGroupKeys.NormalizedName)
			.OnAlreadyExists(input => new ProductGroupNameAlreadyExistsException());

		errors.For(ProducerKeys.Id)
			.OnMissing(input => new ProducerNotFoundException(input.Values[0]));

		errors.For(ProducerKeys.Name)
			.OnMissing(input => new ProducerNameNotFoundException(input.Values));

		errors.For(ProductEanKeys.PK)
			.OnAlreadyExists(input
				=> new ProductEanAlreadyExistsException(input.Values[0].ProductId, input.Values[0].Ean));

		errors.For(ProducerSupplierMappingKeys.SupplierName)
			.OnAlreadyExists(input => new ProducersSupplierMappingAlreadyExistsException(
				input.Values[0].NormalizedName,
				input.Values[0].Supplier));

		errors.For(ProductCharacteristicKeys.CharacteristicId)
			.OnAlreadyExists(input => new CharacteristicInUseException(input.Values[0]));

		errors.For(CharacteristicKeys.Id)
			.OnMissing(input => new CharacteristicNotFoundException(input.Values[0]));

		errors.For(ProducerKeys.Name)
			.OnAlreadyExists(input => new ProducerNameAlreadyTakenException(string.Join(", ", input.Values)));
	}
}
