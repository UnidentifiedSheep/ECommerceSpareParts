using Microsoft.EntityFrameworkCore;

namespace Persistence.Interfaces;

public interface IDomainEventCollector
{
	/// <summary>Collects pending entity events without repeating lifecycle callbacks within the current save.</summary>
	void Collect(DbContext? context);

	/// <summary>Clears collection state after the save completes or fails.</summary>
	void Reset(DbContext context);
}
