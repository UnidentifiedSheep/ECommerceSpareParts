using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Product;
using Main.Entities.Exceptions;
using Main.Entities.Product;

namespace Main.Application.Handlers.Characteristics.UpsertCharacteristic;

[AutoSave]
[Transactional]
public record UpsertCharacteristicCommand(int? Id, string Name) : ICommand<UpsertCharacteristicResult>;

public record UpsertCharacteristicResult(CharacteristicDto Characteristic);

public class UpsertCharacteristicHandler(
	IUnitOfWork unitOfWork,
	IRepository<Characteristic, int> repository,
	IProjectionProvider<Characteristic, CharacteristicDto> projection)
	: ICommandHandler<UpsertCharacteristicCommand, UpsertCharacteristicResult>
{
	public async Task<UpsertCharacteristicResult> Handle(
		UpsertCharacteristicCommand request,
		CancellationToken cancellationToken)
	{
		var normalizedName = Characteristic.NormalizeName(request.Name.Trim());
		var characteristics = await repository.ListAsync(
			Criteria<Characteristic>.New()
				.Where(characteristic =>
					characteristic.NormalizedName == normalizedName ||
					characteristic.Id == request.Id)
				.Track()
				.Build(),
			cancellationToken);

		var characteristic = request.Id != null
			? characteristics.FirstOrDefault(item => item.Id == request.Id) ??
				throw new CharacteristicNotFoundException(request.Id.Value)
			: null;

		if (characteristics.Any(item => item.NormalizedName == normalizedName && item.Id != request.Id))
			throw new CharacteristicNameAlreadyExistsException();

		if (characteristic is null)
		{
			characteristic = Characteristic.Create(request.Name);
			await unitOfWork.AddAsync(characteristic, cancellationToken);
		}
		else
			characteristic.SetName(request.Name);

		await unitOfWork.SaveChangesAsync(cancellationToken);
		return new UpsertCharacteristicResult(projection.ProjectionFunc(characteristic));
	}
}
