using Application.Common.Extensions;
using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities;

namespace Main.Modules.Balance.Application.Commands.Balance.CreateTransaction;

public class CreateTransactionValidation : AbstractValidator<CreateTransactionCommand>
{
	public CreateTransactionValidation(IOperationDatePolicy datePolicy)
	{
		RuleFor(command => command.SenderId)
			.NotEmpty()
			.WithLocalizableError(TransactionSenderIdRequiredMessage.Instance);

		RuleFor(command => command.ReceiverId)
			.NotEmpty()
			.WithLocalizableError(TransactionReceiverIdRequiredMessage.Instance);

		RuleFor(command => command.Amount).SetValidator(new TransactionAmountValidator());

		RuleFor(command => command.TransactionDateTime).SetValidator(new RecordDateValidator(datePolicy));
	}
}
