using FluentAssertions;
using Locan.Core.Interfaces;
using Main.Application.Services;
using Main.Entities.Product;
using Main.Modules.Product.Abstractions;

namespace Tests.ServicesTests;

public class ProductDeletionBlockerResolverTests
{
	[Fact]
	public async Task NoRules_ReturnsNull()
		=> (await new ProductDeletionBlockerResolver([]).GetAsync(1, TestContext.Current.CancellationToken))
			.Should()
			.BeNull();

	[Fact]
	public async Task RulesWithoutBlocker_ReturnNull()
		=> (await new ProductDeletionBlockerResolver([new FakeRule(10, null), new FakeRule(20, null)])
				.GetAsync(1, TestContext.Current.CancellationToken))
			.Should()
			.BeNull();

	[Fact]
	public async Task FirstBlockerByOrder_Wins()
	{
		var rules = new IProductDeletionRule[]
		{
			new FakeRule(30, ArticleDeletionSaleMessage.Instance),
			new FakeRule(10, ArticleDeletionStockMessage.Instance),
			new FakeRule(20, ArticleDeletionOrderMessage.Instance)
		};

		var result = await new ProductDeletionBlockerResolver(rules).GetAsync(1, TestContext.Current.CancellationToken);

		result!.MessageKey.Should().Be(ArticleDeletionStockMessage.Key);
	}

	[Fact]
	public async Task LaterRules_AreNotEvaluatedAfterBlocker()
	{
		var late = new FakeRule(20, ArticleDeletionOrderMessage.Instance);
		var rules = new IProductDeletionRule[] { new FakeRule(10, ArticleDeletionStockMessage.Instance), late };

		await new ProductDeletionBlockerResolver(rules).GetAsync(1, TestContext.Current.CancellationToken);

		late.Calls.Should().Be(0);
	}

	private sealed class FakeRule(int order, ILocalizableMessage? message) : IProductDeletionRule
	{
		public int Calls { get; private set; }

		public int Order => order;

		public Task<ILocalizableMessage?> CheckAsync(int productId, CancellationToken cancellationToken = default)
		{
			Calls++;
			return Task.FromResult(message);
		}
	}
}
