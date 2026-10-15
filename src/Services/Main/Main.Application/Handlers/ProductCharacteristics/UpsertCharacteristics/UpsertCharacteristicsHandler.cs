using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using MediatR;

namespace Main.Application.Handlers.ProductCharacteristics.UpsertCharacteristics;

[AutoSave]
[Transactional]
public record UpsertCharacteristicsCommand(
	IReadOnlyCollection<UpsertCharacteristicsDto> Characteristics) : ICommand;

public class UpsertCharacteristicsHandler(
	IRepository<ProductCharacteristic, (int, int)> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<UpsertCharacteristicsCommand>
{
	public async Task<Unit> Handle(UpsertCharacteristicsCommand request, CancellationToken cancellationToken)
	{
		if (request.Characteristics.Count == 0)
			return Unit.Value;

		var productIds = request.Characteristics
			.Select(x => x.ProductId)
			.Distinct()
			.ToList();

		var characteristicIds = request.Characteristics
			.Select(x => x.CharacteristicId)
			.Distinct()
			.ToList();

		var existing = (await repository.ListAsync(
			Criteria<ProductCharacteristic>.New()
				.Where(x => productIds.Contains(x.ProductId) && characteristicIds.Contains(x.CharacteristicId))
				.Track()
				.Build(),
			cancellationToken))
			.ToDictionary(x => x.GetId());

		var toAdd = new List<ProductCharacteristic>();
		foreach (var characteristic in request.Characteristics)
			if (existing.TryGetValue((characteristic.ProductId, characteristic.CharacteristicId), out var entity))
				entity.SetValue(characteristic.Value);
			else
				toAdd.Add(ProductCharacteristic.Create(
					characteristic.ProductId,
					characteristic.CharacteristicId,
					characteristic.Value));

		if (toAdd.Count > 0)
			await unitOfWork.AddRangeAsync(toAdd, cancellationToken);

		return Unit.Value;
	}
}
