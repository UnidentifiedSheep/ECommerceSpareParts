using FluentValidation;

namespace Main.Modules.Product.Application.Commands.Characteristics.DeleteCharacteristic;

public class DeleteCharacteristicValidation : AbstractValidator<DeleteCharacteristicCommand>
{
	public DeleteCharacteristicValidation() => RuleFor(command => command.Id).GreaterThan(0);
}
