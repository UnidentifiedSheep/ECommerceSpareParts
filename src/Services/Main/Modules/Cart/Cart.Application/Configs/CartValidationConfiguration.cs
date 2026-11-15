using BulkValidation.Core.Configuration;
using Main.Entities.Cart;
using Main.Entities.Exceptions;

namespace Main.Application.Configs;

public static class CartValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
	{
		errors.For(CartKeys.PK)
			.OnMissing(input => new CartItemNotFoundException(input.Values[0].ProductId));

		errors.For(CartKeys.PK)
			.OnAlreadyExists(input => new CartItemAlreadyExistsException(input.Values));
	}
}
