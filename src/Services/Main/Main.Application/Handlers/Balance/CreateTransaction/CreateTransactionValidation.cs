using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Balance.CreateTransaction;

public class CreateTransactionValidation : AbstractValidator<CreateTransactionCommand>
{
	public CreateTransactionValidation(IOperationDatePolicy datePolicy)
	{
		RuleFor(command => command.SenderId).NotEmpty().WithLocalizationKey("transaction.sender.id.required");

		RuleFor(command => command.ReceiverId)
			.NotEmpty()
			.WithLocalizationKey("transaction.receiver.id.required");

		RuleFor(command => command.Amount).SetValidator(new TransactionAmountValidator());

		RuleFor(command => command.TransactionDateTime).SetValidator(new RecordDateValidator(datePolicy));
	}
}
