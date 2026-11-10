using Domain;
using Domain.Interfaces.Events;

namespace Tests.Persistence.Entities;

internal sealed class BeforeSaveTestEntity : Entity<BeforeSaveTestEntity, Guid>
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Value { get; set; } = "original";
	public int CreatedCalls { get; private set; }
	public void Raise(IDomainEvent item) => AddDomainEvent(item);
	public override Guid GetId() => Id;
	public override void OnCreated() => CreatedCalls++;
}
