using FluentAssertions;
using FluentValidation.Results;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Analyzers.CoPurchase;

namespace Tests.AnalyticsTests;

public class CoPurchaseParametersValidatorTests
{
	private static readonly DateTime From = new(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);

	private static CoPurchaseParameters Parameters()
		=> new()
		{
			From = From,
			To = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
		};

	private static ValidationResult Validate(CoPurchaseParameters parameters)
		=> new CoPurchaseParametersValidator().Validate(parameters);

	[Fact]
	public void Validator_AcceptsDefaultParameters()
		=> Validate(Parameters()).IsValid.Should().BeTrue();

	[Fact]
	public void Validator_AcceptsSingleMonthWindow()
		=> Validate(Parameters() with { To = From }).IsValid.Should().BeTrue();

	[Fact]
	public void Validator_AcceptsCandidatesEqualToResult()
		=> Validate(Parameters() with { CandidatesTopK = 5, ResultTopN = 5 }).IsValid.Should().BeTrue();

	[Theory]
	[MemberData(nameof(InvalidParameters))]
	public void Validator_RejectsInvalidParameters(CoPurchaseParameters parameters)
		=> Validate(parameters).IsValid.Should().BeFalse();

	public static TheoryData<CoPurchaseParameters> InvalidParameters()
		=> new()
		{
			Parameters() with { From = new DateTime(2025, 12, 2, 0, 0, 0, DateTimeKind.Utc) },
			Parameters() with { From = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Local) },
			Parameters() with { To = new DateTime(2026, 11, 1, 1, 0, 0, DateTimeKind.Utc) },
			Parameters() with { To = From.AddMonths(-1) },
			Parameters() with { MinPairCount = 0 },
			Parameters() with { SuspiciousCrossMinPairCount = 0 },
			Parameters() with { SuspiciousCrossMinLift = 1m },
			Parameters() with { ResultTopN = 0 },
			Parameters() with { CandidatesTopK = 9, ResultTopN = 10 }
		};
}
