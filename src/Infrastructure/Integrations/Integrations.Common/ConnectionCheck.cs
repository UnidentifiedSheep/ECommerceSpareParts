
namespace Integrations.Common;

public record ConnectionCheck<TConnection>(
	bool CanUse,
	TConnection? Connection,
	ConnectionUnavailableReason? Reason = null,
	string? Message = null) : ConnectionCheck(
	CanUse,
	Reason,
	Message);

public record ConnectionCheck(bool CanUse, ConnectionUnavailableReason? Reason = null, string? Message = null);
