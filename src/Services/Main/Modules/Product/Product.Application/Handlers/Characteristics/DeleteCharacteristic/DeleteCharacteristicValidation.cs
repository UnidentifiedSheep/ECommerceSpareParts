using FluentValidation;

namespace Main.Modules.Product.Application.Handlers.Characteristics.DeleteCharacteristic;

public class DeleteCharacteristicValidation : AbstractValidator<DeleteCharacteristicCommand>
{
	public DeleteCharacteristicValidation() => RuleFor(command => command.Id).GreaterThan(0);
}
