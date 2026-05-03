using Exceptions;
using FluentAssertions;
using Main.Entities.Producer;
using Main.Entities.Producer.ValueObjects;

namespace Main.Tests.Domain.Producer;

public class ProducerOtherNameTests
{
    [Theory]
    [InlineData(1, "KSS", "EU")]
    [InlineData(2, "RjkbenSchmidt", "US")]
    [InlineData(3, "  Test alias  ", "eu")]
    public void Create_ValidData_Succeeds(int producerId, string otherName, string whereUsed)
    {
        var entity = ProducerOtherName.Create(producerId, otherName, whereUsed);

        entity.ProducerId.Should().Be(producerId);
        entity.OtherName.Should().Be(otherName.Trim());
        entity.WhereUsed.Should().Be(whereUsed.Trim().ToUpperInvariant());
    }

    [Fact]
    public void Create_EmptyWhereUsed_Throws()
    {
        var act = () => ProducerOtherName.Create(1, "KSS", "   ");

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetWhereUsed_Valid_UpdatesValue()
    {
        var entity = ProducerOtherName.Create(1, "KSS", "EU");

        entity.SetWhereUsed("us");

        entity.WhereUsed.Should().Be("US");
    }

    [Fact]
    public void SetWhereUsed_TooShort_Throws()
    {
        var entity = ProducerOtherName.Create(1, "KSS", "EU");

        var act = () => entity.SetWhereUsed("a");

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetOtherName_AllowsAnyValue()
    {
        var entity = ProducerOtherName.Create(1, "KSS", "EU");

        entity.SetOtherName(new Name("Samsung"));

        entity.OtherName.Should().Be("Samsung");
    }

    [Fact]
    public void Key_IsStableAfterNormalization()
    {
        var entity = ProducerOtherName.Create(1, "  KSS  ", "eu");

        var key = entity.GetId();

        key.ProducerId.Should().Be(1);
        key.OtherName.Should().Be("KSS");
        key.WhereUsed.Should().Be("EU");
    }
}