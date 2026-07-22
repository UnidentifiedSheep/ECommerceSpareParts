using Enums;
using FluentAssertions;
using Main.Entities.Purchase;
using Main.Enums;
using DomainPurchase = Main.Entities.Purchase.Purchase;

namespace Tests.Domain.Purchase;

public class PurchaseTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var supplierUserId = Guid.NewGuid();
        var supplierOrganizationId = Guid.NewGuid();
        var purchase = DomainPurchase.Create(
            supplierUserId,
            supplierOrganizationId,
            1,
            Guid.NewGuid(),
            "WH-1",
            DateTime.UtcNow);

        purchase.State.Should().Be(PurchaseState.Draft);
        purchase.SupplierUserId.Should().Be(supplierUserId);
        purchase.SupplierOrganizationId.Should().Be(supplierOrganizationId);
    }

    [Fact]
    public void Complete_ChangesState()
    {
        var purchase = Create();

        purchase.Complete();

        purchase.State.Should().Be(PurchaseState.Completed);
    }

    [Fact]
    public void AddContent_WithWrongPurchaseId_Throws()
    {
        var purchase = Create();

        var content = PurchaseContent.Create(
            1,
            2,
            10m,
            3);

        typeof(PurchaseContent)
            .GetProperty("PurchaseId")!
            .SetValue(content, Guid.NewGuid());

        var act = () => purchase.AddContent(content);

        act.Should().Throw<InvalidOperationException>();
    }

    private static DomainPurchase Create()
    {
        return DomainPurchase.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            "WH-1",
            DateTime.UtcNow);
    }
}
