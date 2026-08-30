using Abstractions.Interfaces;
using Application.Common.Services;
using Enums;
using Extensions;
using FluentAssertions;
using Moq;

namespace Tests.Tests.Services;

public class OperationDatePolicyTests
{
	private static readonly DateTime Now = new(
		2026,
		7,
		31,
		12,
		0,
		0,
		DateTimeKind.Utc);

	[Fact]
	public void IsAllowed_DateAtFutureClockSkewBoundary_ReturnsValid()
	{
		var policy = CreatePolicy();

		var result = policy.IsAllowed(Now.AddMinutes(5));

		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void IsAllowed_DateBeyondFutureClockSkew_ReturnsFutureError()
	{
		var policy = CreatePolicy();

		var result = policy.IsAllowed(Now.AddMinutes(5).AddTicks(1));

		result.IsValid.Should().BeFalse();
		result.Message.Should().Be("operation.date.cannot.be.in.future");
	}

	[Fact]
	public void IsAllowed_DateAtBackdateBoundaryWithoutPermission_ReturnsValid()
	{
		var policy = CreatePolicy();

		var result = policy.IsAllowed(Now.AddDays(-30));

		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void IsAllowed_DateBeyondBackdateBoundaryWithoutPermission_ReturnsTooOldError()
	{
		var policy = CreatePolicy();

		var result = policy.IsAllowed(Now.AddDays(-30).AddTicks(-1));

		result.IsValid.Should().BeFalse();
		result.Message.Should().Be("operation.date.too.old");
	}

	[Fact]
	public void IsAllowed_OldDateWithHistoricalRecordsPermission_ReturnsValid()
	{
		var permission = nameof(PermissionCodes.CREATE_HISTORICAL_RECORDS).ToNormalizedPermission();
		var policy = CreatePolicy(
			new HashSet<string>
			{
				permission
			});

		var result = policy.IsAllowed(Now.AddYears(-1));

		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void IsAllowed_FutureDateWithHistoricalRecordsPermission_ReturnsFutureError()
	{
		var permission = nameof(PermissionCodes.CREATE_HISTORICAL_RECORDS).ToNormalizedPermission();
		var policy = CreatePolicy(
			new HashSet<string>
			{
				permission
			});

		var result = policy.IsAllowed(Now.AddMinutes(6));

		result.IsValid.Should().BeFalse();
		result.Message.Should().Be("operation.date.cannot.be.in.future");
	}

	private static OperationDatePolicy CreatePolicy(IReadOnlySet<string>? permissions = null)
	{
		var timeProvider = new Mock<TimeProvider>();
		timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(Now));

		var userContext = new Mock<IUserContext>();
		userContext.SetupGet(x => x.Permissions).Returns(permissions ?? new HashSet<string>());

		return new OperationDatePolicy(timeProvider.Object, userContext.Object);
	}
}
