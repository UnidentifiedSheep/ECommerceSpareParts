using Application.Common.Services;
using Application.Common.Validators;
using FluentAssertions;
using Moq;

namespace Tests.Tests.Validators;

public class RecordDateValidatorTests
{
    [Fact]
    public void Validate_AllowedDate_PassesUtcDateToPolicyAndReturnsValid()
    {
        var date = new DateTime(
            2026,
            7,
            31,
            12,
            0,
            0,
            DateTimeKind.Local);
        var utcDate = date.ToUniversalTime();
        var policy = new Mock<IOperationDatePolicy>();
        policy
            .Setup(x => x.IsAllowed(utcDate))
            .Returns(OperationDateValidationResult.Valid());
        var validator = new RecordDateValidator(policy.Object);

        var result = validator.Validate(date);

        result.IsValid.Should().BeTrue();
        policy.Verify(x => x.IsAllowed(utcDate), Times.Once);
    }

    [Fact]
    public void Validate_DisallowedDate_UsesPolicyMessageAsErrorCode()
    {
        const string errorCode = "operation.date.too.old";
        var date = new DateTime(
            2026,
            6,
            1,
            12,
            0,
            0,
            DateTimeKind.Utc);
        var policy = new Mock<IOperationDatePolicy>();
        policy
            .Setup(x => x.IsAllowed(date))
            .Returns(OperationDateValidationResult.Invalid(errorCode));
        var validator = new RecordDateValidator(policy.Object);

        var result = validator.Validate(date);

        result.Errors.Should().ContainSingle()
            .Which.ErrorCode.Should().Be(errorCode);
    }
}
