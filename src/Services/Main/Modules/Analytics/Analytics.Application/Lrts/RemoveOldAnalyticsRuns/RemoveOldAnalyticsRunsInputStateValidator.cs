using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Modules.Analytics.Application.Lrts.RemoveOldAnalyticsRuns;

public sealed class RemoveOldAnalyticsRunsInputStateValidator : AbstractValidator<RemoveOldAnalyticsRunsInputState>
{
	public RemoveOldAnalyticsRunsInputStateValidator()
		=> RuleFor(x => x.OlderThanDays)
			.GreaterThanOrEqualTo(1)
			.WithLocalizableError(RemoveOldAnalyticsRunsOlderThanDaysInvalidMessage.Instance);
}
