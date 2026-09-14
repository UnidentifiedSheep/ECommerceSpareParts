using Abstractions.Interfaces;
using Enums;
using Extensions;
using Locan.Core.Interfaces;

namespace Application.Common.Services;

public interface IOperationDatePolicy
{
	OperationDateValidationResult IsAllowed(DateTime occurredAtUtc);
}

public sealed class OperationDatePolicy(TimeProvider timeProvider, IUserContext userContext)
	: IOperationDatePolicy
{
	private static readonly TimeSpan AllowedClockSkew = TimeSpan.FromMinutes(5);

	private static readonly TimeSpan DefaultBackdatePeriod = TimeSpan.FromDays(30);

	public OperationDateValidationResult IsAllowed(DateTime occurredAtUtc)
	{
		var now = timeProvider.GetUtcNow();

		if (occurredAtUtc > now + AllowedClockSkew)
			return OperationDateValidationResult.Invalid(OperationDateCannotBeInFutureMessage.Instance);

		var allowHistory = userContext.Permissions.Contains(
			nameof(PermissionCodes.CREATE_HISTORICAL_RECORDS).ToNormalizedPermission());
		if (!allowHistory && occurredAtUtc < now - DefaultBackdatePeriod)
			return OperationDateValidationResult.Invalid(OperationDateTooOldMessage.Instance);

		return OperationDateValidationResult.Valid();
	}
}

public record OperationDateValidationResult
{
	public bool IsValid { get; init; }

	public ILocalizableMessage? LocalizableMessage { get; init; }

	public static OperationDateValidationResult Valid() => new()
	{
		IsValid = true
	};

	public static OperationDateValidationResult Invalid(ILocalizableMessage message) => new()
	{
		LocalizableMessage = message
	};
}
