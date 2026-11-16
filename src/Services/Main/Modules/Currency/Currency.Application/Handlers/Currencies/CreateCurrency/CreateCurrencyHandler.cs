using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Currencies.CreateCurrency;

[AutoSave]
[Transactional]
public record CreateCurrencyCommand(
	string ShortName,
	string Name,
	string CurrencySign,
	string Code) : ICommand<CreateCurrencyResult>;

public record CreateCurrencyResult(int Id);

public class CreateCurrencyHandler(IUnitOfWork unitOfWork)
	: ICommandHandler<CreateCurrencyCommand, CreateCurrencyResult>
{
	public async Task<CreateCurrencyResult> Handle(
		CreateCurrencyCommand request,
		CancellationToken cancellationToken)
	{
		var model = Currency.Create(
			request.Name,
			request.ShortName,
			request.CurrencySign,
			request.Code);
		await unitOfWork.AddAsync(model, cancellationToken);

		return new CreateCurrencyResult(model.Id);
	}
}
