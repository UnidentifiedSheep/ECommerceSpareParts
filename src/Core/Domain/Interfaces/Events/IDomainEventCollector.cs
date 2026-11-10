namespace Domain.Interfaces.Events;

public interface IDomainEventCollector
{
	void Collect();

	void Reset();
}
