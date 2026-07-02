using FluentAssertions;
using Main.Entities.Producer;
using ProducerDomain = Main.Entities.Producer.Producer;

namespace Tests.Domain.Producer;

public class ProducerAliasTests
{
    [Theory]
    [InlineData(
        1,
        "KSS")]
    [InlineData(
        2,
        "RjkbenSchmidt")]
    [InlineData(
        3,
        "  Test alias  ")]
    public void Create_ValidData_Succeeds(
        int producerId,
        string otherName)
    {
        var entity = ProducerAlias.Create(
            producerId,
            otherName);

        entity.ProducerId.Should().Be(producerId);
        entity.Alias.Should().Be(ProducerDomain.ToNormalizedName(otherName));
    }

    [Fact]
    public void Create_EmptyWhereUsed_Throws()
    {
        var act = () => ProducerAlias.Create(
            1,
            "KSS");

        act.Should().NotThrow();
    }

    [Fact]
    public void SetAlias_NormalizesValue()
    {
        var entity = ProducerAlias.Create(
            1,
            "KSS");

        entity.SetAlias("Samsung");

        entity.Alias.Should().Be("SAMSUNG");
    }

    [Fact]
    public void Key_IsNormalizedAlias()
    {
        var entity = ProducerAlias.Create(
            1,
            "  KSS  ");

        var key = entity.GetId();

        key.Should().Be("KSS");
    }
}