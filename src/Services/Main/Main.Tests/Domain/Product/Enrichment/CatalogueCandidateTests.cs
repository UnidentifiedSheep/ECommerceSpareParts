using Domain.Events;
using Enums;
using FluentAssertions;
using Main.Entities.DomainEvents.CatalogueCandidate;
using Main.Entities.Product.Enrichment;
using Main.Entities.Product.ValueObjects;

namespace Tests.Domain.Product.Enrichment;

public sealed class CatalogueCandidateTests
{
	[Fact]
	public void Create_ValidData_CreatesCandidate()
	{
		var candidate = CatalogueCandidate.Create(" ABC-123 ", 42);

		candidate.Id.Should().NotBeEmpty();
		candidate.Id.Version.Should().Be(7);
		candidate.Sku.Value.Should().Be("ABC-123");
		candidate.Sku.NormalizedValue.Should().Be("ABC123");
		candidate.ProducerId.Should().Be(42);
		candidate.ProductId.Should().BeNull();
		candidate.SupplierProducts.Should().BeEmpty();
		candidate.FlushDomainEvents().Should().BeEmpty();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Create_InvalidProducerId_Throws(int producerId)
	{
		var action = () => CatalogueCandidate.Create("ABC-123", producerId);

		action.Should().Throw<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("producerId");
	}

	[Fact]
	public void MapToProduct_ValidProductId_UpdatesProductId()
	{
		var candidate = CreateCandidate();

		candidate.MapToProduct(25);

		candidate.ProductId.Should().Be(25);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void MapToProduct_InvalidProductId_Throws(int productId)
	{
		var candidate = CreateCandidate();

		var action = () => candidate.MapToProduct(productId);

		action.Should().Throw<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("productId");
		candidate.ProductId.Should().BeNull();
	}

	[Fact]
	public void RemoveProductMapping_MappedCandidate_ClearsProductId()
	{
		var candidate = CreateCandidate();
		candidate.MapToProduct(25);

		candidate.RemoveProductMapping();

		candidate.ProductId.Should().BeNull();
	}

	[Fact]
	public void AddSupplierProduct_NewProduct_AddsProductAndUpdatedEvent()
	{
		var candidate = CreateCandidate();
		var supplierProduct = CreateSupplierProduct();

		candidate.AddSupplierProduct(supplierProduct);

		candidate.SupplierProducts.Should().ContainSingle().Which.Should().BeSameAs(supplierProduct);
		candidate
			.FlushDomainEvents()
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.Be(new CatalogueCandidateContentChangedDomainEvent(candidate.Id));
	}

	[Fact]
	public void AddSupplierProduct_NullProduct_Throws()
	{
		var candidate = CreateCandidate();

		var action = () => candidate.AddSupplierProduct(null!);

		action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("supplierProduct");
		candidate.SupplierProducts.Should().BeEmpty();
		candidate.FlushDomainEvents().Should().BeEmpty();
	}

	[Fact]
	public void AddSupplierProduct_ExistingProduct_DoesNothing()
	{
		var candidate = CreateCandidate();
		var supplierProduct = CreateSupplierProduct();
		candidate.AddSupplierProduct(supplierProduct);
		candidate.FlushDomainEvents();

		candidate.AddSupplierProduct(supplierProduct);

		candidate.SupplierProducts.Should().ContainSingle();
		candidate.FlushDomainEvents().Should().BeEmpty();
	}

	[Fact]
	public void AddSupplierProduct_MultipleProducts_AddsSingleUpdatedEvent()
	{
		var candidate = CreateCandidate();
		var firstProduct = CreateSupplierProduct("FIRST-123");
		var secondProduct = CreateSupplierProduct("SECOND-123");

		candidate.AddSupplierProduct(firstProduct);
		candidate.AddSupplierProduct(secondProduct);

		candidate.SupplierProducts.Should().Equal(firstProduct, secondProduct);
		candidate
			.FlushDomainEvents()
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.Be(new CatalogueCandidateContentChangedDomainEvent(candidate.Id));
	}

	[Fact]
	public void RemoveSupplierProduct_ExistingProduct_RemovesProductAndAddsUpdatedEvent()
	{
		var candidate = CreateCandidate();
		var supplierProduct = CreateSupplierProduct();
		candidate.AddSupplierProduct(supplierProduct);
		candidate.FlushDomainEvents();

		candidate.RemoveSupplierProduct(supplierProduct);

		candidate.SupplierProducts.Should().BeEmpty();
		candidate
			.FlushDomainEvents()
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.Be(new CatalogueCandidateContentChangedDomainEvent(candidate.Id));
	}

	[Fact]
	public void RemoveSupplierProduct_NullProduct_Throws()
	{
		var candidate = CreateCandidate();

		var action = () => candidate.RemoveSupplierProduct(null!);

		action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("supplierProduct");
		candidate.FlushDomainEvents().Should().BeEmpty();
	}

	[Fact]
	public void RemoveSupplierProduct_MissingProduct_DoesNothing()
	{
		var candidate = CreateCandidate();

		candidate.RemoveSupplierProduct(CreateSupplierProduct());

		candidate.SupplierProducts.Should().BeEmpty();
		candidate.FlushDomainEvents().Should().BeEmpty();
	}

	[Fact]
	public void GetId_ReturnsCandidateId()
	{
		var candidate = CreateCandidate();

		var id = candidate.GetId();

		id.Should().Be(candidate.Id);
	}

	[Fact]
	public void GetKeySelector_ReturnsCandidateId()
	{
		var candidate = CreateCandidate();

		var id = CatalogueCandidate.GetKeySelector().Compile()(candidate);

		id.Should().Be(candidate.Id);
	}

	[Fact]
	public void GetEqualityExpression_MatchingId_ReturnsTrue()
	{
		var candidate = CreateCandidate();
		var predicate = CatalogueCandidate.GetEqualityExpression(candidate.Id).Compile();

		var result = predicate(candidate);

		result.Should().BeTrue();
	}

	[Fact]
	public void GetEqualityExpression_DifferentId_ReturnsFalse()
	{
		var candidate = CreateCandidate();
		var predicate = CatalogueCandidate.GetEqualityExpression(Guid.CreateVersion7()).Compile();

		var result = predicate(candidate);

		result.Should().BeFalse();
	}

	[Fact]
	public void OnCreated_AddsCreatedEvent()
	{
		var candidate = CreateCandidate();

		candidate.OnCreated();

		var domainEvent = candidate
			.FlushDomainEvents()
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.BeOfType<EntityCreatedDomainEvent<CatalogueCandidate>>()
			.Which;
		domainEvent.Entity.Should().BeSameAs(candidate);
	}

	[Fact]
	public void OnUpdated_AddsUpdatedEvent()
	{
		var candidate = CreateCandidate();

		candidate.OnUpdated();

		candidate
			.FlushDomainEvents()
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.Be(new EntityUpdatedDomainEvent<CatalogueCandidate, Guid>(candidate.Id));
	}

	[Fact]
	public void OnDeleted_AddsDeletedEvent()
	{
		var candidate = CreateCandidate();

		candidate.OnDeleted();

		candidate
			.FlushDomainEvents()
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.Be(new EntityDeletedDomainEvent<CatalogueCandidate, Guid>(candidate.Id));
	}

	private static CatalogueCandidate CreateCandidate() => CatalogueCandidate.Create("ABC-123", 42);

	private static SupplierProduct CreateSupplierProduct(string sku = "SUPPLIER-123")
	{
		return SupplierProduct.Create(
			new Sku(sku),
			"Supplier producer",
			Supplier.FavoritParts);
	}
}
