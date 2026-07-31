using Abstractions.Interfaces;
using Enums;
using Extensions;

namespace Application.Common.Services;

public interface IOperationDatePolicy
{
    OperationDateValidationResult IsAllowed(DateTime occurredAtUtc);
}

public sealed class OperationDatePolicy(
    TimeProvider timeProvider,
    IUserContext userContext) : IOperationDatePolicy
{
    private static readonly TimeSpan AllowedClockSkew = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DefaultBackdatePeriod = TimeSpan.FromDays(30);

    public OperationDateValidationResult IsAllowed(DateTime occurredAtUtc)
    {
        var now = timeProvider.GetUtcNow();

        if (occurredAtUtc > now + AllowedClockSkew)
            return OperationDateValidationResult.Invalid(
                "operation.date.cannot.be.in.future");
        

        var allowHistory = userContext.Permissions
            .Contains(nameof(PermissionCodes.CREATE_HISTORICAL_RECORDS)
                .ToNormalizedPermission());
        if (!allowHistory && occurredAtUtc < now - DefaultBackdatePeriod)
            return OperationDateValidationResult.Invalid(
                "operation.date.too.old");

        return OperationDateValidationResult.Valid();
    }
}

public record OperationDateValidationResult
{
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;

    public static OperationDateValidationResult Valid() => new() { IsValid = true };
    public static OperationDateValidationResult Invalid(string message) => new() { Message = message };
}
