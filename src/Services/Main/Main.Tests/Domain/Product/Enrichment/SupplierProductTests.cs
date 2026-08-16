using Enums;
using FluentAssertions;
using Main.Entities.DomainEvents.CatalogueCandidate;
using Main.Entities.Product.Enrichment;
using Main.Entities.Product.ValueObjects;

namespace Tests.Domain.Product.Enrichment;

public sealed class SupplierProductTests
{
    [Fact]
    public void Create_ValidData_CreatesSupplierProduct()
    {
        var supplierProduct = SupplierProduct.Create(
            new Sku("SUPPLIER-123"),
            "  Supplier producer  ",
            Supplier.FavoritParts);

        supplierProduct.Id.Should().Be(0);
        supplierProduct.Sku.Value.Should().Be("SUPPLIER-123");
        supplierProduct.Sku.NormalizedValue.Should().Be("SUPPLIER123");
        supplierProduct.Producer.Should().Be("Supplier producer");
        supplierProduct.Supplier.Should().Be(Supplier.FavoritParts);
        supplierProduct.CatalogueCandidateId.Should().BeNull();
        supplierProduct.CatalogueCandidate.Should().BeNull();
        supplierProduct.Names.Should().BeEmpty();
        supplierProduct.FlushDomainEvents().Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidProducer_Throws(string? producer)
    {
        var action = () => SupplierProduct.Create(
            new Sku("SUPPLIER-123"),
            producer!,
            Supplier.FavoritParts);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddName_NewName_AddsNormalizedName()
    {
        var supplierProduct = CreateSupplierProduct();

        supplierProduct.AddName("  Oil filter  ");

        var name = supplierProduct.Names.Should().ContainSingle().Which;
        name.Name.Should().Be("Oil filter");
        name.SupplierProductId.Should().Be(supplierProduct.Id);
    }

    [Fact]
    public void AddName_ProductWithoutCandidate_DoesNotAddCandidateEvent()
    {
        var supplierProduct = CreateSupplierProduct();

        supplierProduct.AddName("Oil filter");

        supplierProduct.FlushDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void AddName_ProductWithCandidate_AddsContentChangedEvent()
    {
        var candidate = CreateCandidate();
        var supplierProduct = CreateSupplierProduct(candidate.Id);

        supplierProduct.AddName("Oil filter");

        supplierProduct.FlushDomainEvents().Should().ContainSingle()
            .Which.Should().Be(
                new CatalogueCandidateContentChangedDomainEvent(candidate.Id));
    }

    [Fact]
    public void AddName_MultipleNames_AddsSingleContentChangedEvent()
    {
        var candidate = CreateCandidate();
        var supplierProduct = CreateSupplierProduct(candidate.Id);

        supplierProduct.AddName("Oil filter");
        supplierProduct.AddName("Air filter");

        supplierProduct.Names.Select(x => x.Name).Should().Equal(
            "Oil filter",
            "Air filter");
        supplierProduct.FlushDomainEvents().Should().ContainSingle()
            .Which.Should().Be(
                new CatalogueCandidateContentChangedDomainEvent(candidate.Id));
    }

    [Fact]
    public void AddName_DuplicateName_DoesNothing()
    {
        var candidate = CreateCandidate();
        var supplierProduct = CreateSupplierProduct(candidate.Id);
        supplierProduct.AddName("Oil filter");
        supplierProduct.FlushDomainEvents();

        supplierProduct.AddName("  OIL FILTER  ");

        supplierProduct.Names.Should().ContainSingle();
        supplierProduct.FlushDomainEvents().Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddName_InvalidName_Throws(string? name)
    {
        var supplierProduct = CreateSupplierProduct();

        var action = () => supplierProduct.AddName(name!);

        action.Should().Throw<InvalidOperationException>();
        supplierProduct.Names.Should().BeEmpty();
        supplierProduct.FlushDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void GetId_ReturnsSupplierProductId()
    {
        var supplierProduct = CreateSupplierProduct();

        var id = supplierProduct.GetId();

        id.Should().Be(supplierProduct.Id);
    }

    [Fact]
    public void GetKeySelector_ReturnsSupplierProductId()
    {
        var supplierProduct = CreateSupplierProduct();

        var id = SupplierProduct.GetKeySelector()
            .Compile()(supplierProduct);

        id.Should().Be(supplierProduct.Id);
    }

    [Fact]
    public void GetEqualityExpression_MatchingId_ReturnsTrue()
    {
        var supplierProduct = CreateSupplierProduct();
        var predicate = SupplierProduct
            .GetEqualityExpression(supplierProduct.Id)
            .Compile();

        var result = predicate(supplierProduct);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetEqualityExpression_DifferentId_ReturnsFalse()
    {
        var supplierProduct = CreateSupplierProduct();
        var predicate = SupplierProduct
            .GetEqualityExpression(42)
            .Compile();

        var result = predicate(supplierProduct);

        result.Should().BeFalse();
    }

    private static SupplierProduct CreateSupplierProduct(
        Guid? catalogueCandidateId = null)
    {
        var supplierProduct = SupplierProduct.Create(
            new Sku("SUPPLIER-123"),
            "Supplier producer",
            Supplier.FavoritParts);

        //i dont wanna create internal set etc. that's why i have used reflection.
        if (catalogueCandidateId.HasValue)
            typeof(SupplierProduct)
                .GetProperty(nameof(SupplierProduct.CatalogueCandidateId))!
                .SetValue(supplierProduct, catalogueCandidateId.Value);

        return supplierProduct;
    }

    private static CatalogueCandidate CreateCandidate()
    {
        return CatalogueCandidate.Create("CANDIDATE-123", 42);
    }
}
