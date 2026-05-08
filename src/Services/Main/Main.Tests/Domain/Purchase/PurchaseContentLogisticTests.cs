using FluentAssertions;
using Main.Entities.Purchase;

namespace Main.Tests.Domain.Purchase;

public class PurchaseContentLogisticTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var log = PurchaseContentLogistic.Create(1, 2, 3);

        log.WeightKg.Should().Be(1);
        log.AreaM3.Should().Be(2);
        log.Price.Should().Be(3);
    }

    [Fact]
    public void Create_NegativeWeight_Throws()
    {
        var act = () => PurchaseContentLogistic.Create(-1, 2, 3);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Update_PartialConsistency_AllFieldsUpdatedAtomically()
    {
        var log = PurchaseContentLogistic.Create(1, 2, 3);

        log.Update(10, 20, 30);

        log.WeightKg.Should().Be(10);
        log.AreaM3.Should().Be(20);
        log.Price.Should().Be(30);
    }

    [Fact]
    public void Update_MultipleCalls_KeepsConsistentState()
    {
        var log = PurchaseContentLogistic.Create(1, 2, 3);

        log.Update(5, 6, 7);
        log.Update(8, 9, 10);
        log.Update(11, 12, 13);

        log.WeightKg.Should().Be(11);
        log.AreaM3.Should().Be(12);
        log.Price.Should().Be(13);
    }

    [Fact]
    public void Create_HighPrecisionValues_ShouldBehaveConsistently()
    {
        var log = PurchaseContentLogistic.Create(1.123m, 2.456m, 3.789m);

        log.WeightKg.Should().Be(1.123m);
        log.AreaM3.Should().Be(2.456m);
        log.Price.Should().Be(3.789m);
    }

    [Fact]
    public void Update_ChangesValues()
    {
        var log = PurchaseContentLogistic.Create(1, 2, 3);

        log.Update(5, 6, 7);

        log.WeightKg.Should().Be(5);
        log.AreaM3.Should().Be(6);
        log.Price.Should().Be(7);
    }
}