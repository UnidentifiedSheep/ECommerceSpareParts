using FluentAssertions;
using Locan.Core.Interfaces;
using Main.Entities.Product.Product;
using Main.Modules.Product.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.ProductDeletionRulesTests;

public abstract class ProductDeletionRuleTestsBase<TRule> : IntegrationTest where TRule : IProductDeletionRule
{
	protected ProductDeletionRuleTestsBase(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<ProductTestContext>();

	protected Task<ILocalizableMessage?> Check(int productId)
		=> Scope.ServiceProvider
			.GetServices<IProductDeletionRule>()
			.OfType<TRule>()
			.Single()
			.CheckAsync(productId, CancellationToken);

	protected async Task ShouldBlock(int productId, string messageKey)
		=> (await Check(productId)).Should()
			.NotBeNull()
			.And
			.Subject
			.As<ILocalizableMessage>()
			.MessageKey
			.Should()
			.Be(messageKey);

	protected async Task ShouldNotBlock(int productId) => (await Check(productId)).Should().BeNull();

	protected Task<Product> CreateUnreferencedProduct()
		=> new ProductBuilder(Faker).WithProducerId(GetContext<ProductTestContext>().Products[0].ProducerId)
			.BuildAndAddToDb(Context, CancellationToken);
}
