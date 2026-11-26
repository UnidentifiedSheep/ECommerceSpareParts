using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;

namespace Main.Modules.Analytics.Application.Analyzers.CoPurchase;

public sealed class CoPurchaseParametersValidator : AbstractValidator<CoPurchaseParameters>
{
	public CoPurchaseParametersValidator()
	{
		RuleFor(x => x.From)
			.Must(IsMonthStart)
			.WithLocalizableError(CoPurchaseParametersPeriodStartInvalidMessage.Instance);

		RuleFor(x => x.To)
			.Must(IsMonthStart)
			.WithLocalizableError(CoPurchaseParametersPeriodEndInvalidMessage.Instance);

		RuleFor(x => x)
			.Must(x => x.From <= x.To)
			.WithLocalizableError(CoPurchaseParametersPeriodStartMustNotBeAfterEndMessage.Instance);

		RuleFor(x => x.MinPairCount)
			.GreaterThanOrEqualTo(1)
			.WithLocalizableError(CoPurchaseParametersMinPairCountInvalidMessage.Instance);

		RuleFor(x => x.SuspiciousCrossMinPairCount)
			.GreaterThanOrEqualTo(1)
			.WithLocalizableError(CoPurchaseParametersSuspiciousCrossMinPairCountInvalidMessage.Instance);

		RuleFor(x => x.SuspiciousCrossMinLift)
			.GreaterThan(1)
			.WithLocalizableError(CoPurchaseParametersSuspiciousCrossMinLiftInvalidMessage.Instance);

		RuleFor(x => x.ResultTopN)
			.GreaterThanOrEqualTo(1)
			.WithLocalizableError(CoPurchaseParametersResultTopNInvalidMessage.Instance);

		RuleFor(x => x)
			.Must(x => x.CandidatesTopK >= x.ResultTopN)
			.WithLocalizableError(CoPurchaseParametersCandidatesTopKInvalidMessage.Instance);
	}

	private static bool IsMonthStart(DateTime value)
		=> value is { Day: 1, TimeOfDay.Ticks: 0, Kind: DateTimeKind.Utc };
}
