using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;

namespace Main.Modules.Currency.Application.Commands.Currencies.CreateCurrency;

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
		var model = Entities.Currency.Currency.Create(
			request.Name,
			request.ShortName,
			request.CurrencySign,
			request.Code);
		await unitOfWork.AddAsync(model, cancellationToken);
		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new CreateCurrencyResult(model.Id);
	}
}
