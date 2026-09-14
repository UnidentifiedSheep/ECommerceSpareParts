using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Balance.ReverseTransaction;

public class ReverseTransactionValidation : AbstractValidator<ReverseTransactionCommand>
{
	public ReverseTransactionValidation()
	{
		RuleFor(x => x.TransactionId).NotEmpty().WithLocalizableError(TransactionIdRequiredMessage.Instance);
	}
}
