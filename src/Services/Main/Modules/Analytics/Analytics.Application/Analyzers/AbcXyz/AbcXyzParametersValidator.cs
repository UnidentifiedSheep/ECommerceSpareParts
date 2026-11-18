using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Modules.Analytics.Abstractions.Models;

namespace Main.Modules.Analytics.Application.Analyzers.AbcXyz;

public sealed class AbcXyzParametersValidator : AbstractValidator<AbcXyzParameters>
{
	public AbcXyzParametersValidator()
	{
		RuleFor(x => x.From)
			.Must(IsMonthStart)
			.WithLocalizableError(AbcXyzParametersPeriodStartInvalidMessage.Instance);

		RuleFor(x => x.To)
			.Must(IsMonthStart)
			.WithLocalizableError(AbcXyzParametersPeriodEndInvalidMessage.Instance);

		RuleFor(x => x)
			.Must(x => x.From <= x.To)
			.WithLocalizableError(AbcXyzParametersPeriodStartMustNotBeAfterEndMessage.Instance);

		RuleForEach(x => x.Months)
			.IsInEnum()
			.WithLocalizableError(AbcXyzParametersMonthsInvalidMessage.Instance);

		RuleFor(x => x.AbcMetric)
			.IsInEnum()
			.WithLocalizableError(AbcXyzParametersMetricInvalidMessage.Instance);

		RuleFor(x => x.XyzMetric)
			.IsInEnum()
			.WithLocalizableError(AbcXyzParametersMetricInvalidMessage.Instance);

		RuleFor(x => x)
			.Must(x => x.AShare > 0 && x.BShare > 0 && x.AShare + x.BShare <= 1)
			.WithLocalizableError(AbcXyzParametersSharesInvalidMessage.Instance);

		RuleFor(x => x)
			.Must(x => x.XCoefficient > 0 && x.XCoefficient < x.YCoefficient)
			.WithLocalizableError(AbcXyzParametersCoefficientsInvalidMessage.Instance);

		RuleFor(x => x.MinPeriods)
			.GreaterThanOrEqualTo(2)
			.WithLocalizableError(AbcXyzParametersMinPeriodsInvalidMessage.Instance);

		RuleFor(x => x.Granularity)
			.IsInEnum()
			.WithLocalizableError(AbcXyzParametersGranularityInvalidMessage.Instance);

		RuleFor(x => x)
			.Must(x => x.From.Month % 3 == 1 && x.To.Month % 3 == 0)
			.When(x => x.Granularity == AnalysisPeriodGranularity.Quarter)
			.WithLocalizableError(AbcXyzParametersQuarterBoundsInvalidMessage.Instance);

		RuleFor(x => x.Months)
			.Must(months => months is not { Count: > 0 })
			.When(x => x.Granularity == AnalysisPeriodGranularity.Quarter)
			.WithLocalizableError(AbcXyzParametersQuarterMonthsNotSupportedMessage.Instance);

		RuleFor(x => x.AdiCutoff)
			.GreaterThan(0)
			.WithLocalizableError(AbcXyzParametersAdiCutoffInvalidMessage.Instance);

		RuleFor(x => x.Cv2Cutoff)
			.GreaterThan(0)
			.WithLocalizableError(AbcXyzParametersCv2CutoffInvalidMessage.Instance);

		RuleFor(x => x)
			.Must(x => x.MinPeriods <= x.Periods().Count)
			.When(x => x.MinPeriods >= 2)
			.WithLocalizableError(AbcXyzParametersMinPeriodsExceedWindowMessage.Instance);
	}

	private static bool IsMonthStart(DateTime value)
		=> value is { Day: 1, TimeOfDay.Ticks: 0, Kind: DateTimeKind.Utc };
}
