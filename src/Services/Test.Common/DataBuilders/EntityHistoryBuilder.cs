using System.Text.Json;
using Domain.CommonEntities.History;
using Domain.CommonEnums;

namespace Tests.DataBuilders;

internal sealed class EntityHistoryBuilder
{
	private JsonElement _changes = JsonSerializer.SerializeToElement(new { State = new { old = "Draft", @new = "Confirmed" } });
	private EntityHistoryReason[] _reasons = [];

	public EntityHistoryBuilder WithChanges(JsonElement changes)
	{
		_changes = changes;
		return this;
	}

	public EntityHistoryBuilder WithReasons(params EntityHistoryReason[] reasons)
	{
		_reasons = reasons;
		return this;
	}

	public EntityHistory Build() => EntityHistory.Create(
		"Order", "42", "Order", "42", EntityHistoryOperation.Update, _changes,
		Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), _reasons);
}
