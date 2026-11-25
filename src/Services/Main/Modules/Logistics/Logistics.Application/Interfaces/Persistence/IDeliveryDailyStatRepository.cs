using Main.Entities.Logistics.Statistics;

namespace Main.Modules.Logistics.Application.Interfaces.Persistence;

public interface IDeliveryDailyStatRepository
{
	Task UpsertAsync(IReadOnlyCollection<DeliveryDailyStat> stats, CancellationToken cancellationToken = default);
}
