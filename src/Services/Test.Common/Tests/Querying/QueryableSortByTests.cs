using Application.Common.Querying;
using FluentAssertions;
using GraphQL.Common.Extensions;
using GraphQL.Common.Types;

namespace Tests.Tests.Querying;

public sealed class QueryableSortByTests
{
	[Theory]
	[InlineData("count", "count", false, null)]
	[InlineData("count_desc", "count", true, null)]
	[InlineData("count_asc", "count", false, null)]
	[InlineData(" COUNT_Main_Warehouse_DESC ", "count", true, "Main_Warehouse")]
	[InlineData("count_MainWarehouse_asc", "count", false, "MainWarehouse")]
	[InlineData("count_склад-1_desc", "count", true, "склад-1")]
	public void Parse_PreservesArgumentAndReadsDirection(
		string text, string field, bool descending, string? argument)
	{
		SortExpression.Parse(text).Should().Be(new SortExpression(field, descending, argument));
	}

	[Theory]
	[InlineData("count__desc")]
	[InlineData("count_Main_sideways")]
	[InlineData("count_Main_")]
	[InlineData("_desc")]
	public void Parse_RejectsInvalidFormat(string text)
	{
		var action = () => SortExpression.Parse(text);
		action.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void ParameterizedMapping_BindsInsideNestedLambdaAndKeepsOrdinaryMapping()
	{
		var sorting = new QueryableSortBy()
			.Map<Item, long>("count", item => item.Stocks.Sum(stock => (long)stock.Quantity))
			.MapParameterized<Item, string, long>("count", (item, storage) => item.Stocks
				.Where(stock => stock.Code == storage).Sum(stock => (long)stock.Quantity));
		var items = new[] { new Item(1, [new Stock("Main", 3), new Stock("Other", 7)]) }.AsQueryable();

		items.Select(sorting.GetMapping<Item>("count")).Single().Should().Be(10L);
		items.Select(sorting.GetMapping<Item>("count", "Main")).Single().Should().Be(3L);
		items.Select(sorting.GetMapping<Item>("count", "Other")).Single().Should().Be(7L);
		items.Select(sorting.GetMapping<Item>("count", "main")).Single().Should().Be(0L);
		items.Select(sorting.GetMapping<Item>("count", "Main")).Single().Should().Be(3L);
	}

	[Fact]
	public void ParameterizedMapping_ConvertsIntDecimalGuidAndEnum()
	{
		var guid = Guid.NewGuid();
		var sorting = new QueryableSortBy()
			.MapParameterized<Item, int, int>("offset", (item, offset) => item.Id + offset)
			.MapParameterized<Item, decimal, decimal>("factor", (item, factor) => item.Id * factor)
			.MapParameterized<Item, Guid, Guid>("owner", (item, owner) => owner)
			.MapParameterized<Item, TestState, TestState>("state", (item, state) => state);
		var items = new[] { new Item(2, []) }.AsQueryable();

		items.Select(sorting.GetMapping<Item>("offset", "3")).Single().Should().Be(5);
		items.Select(sorting.GetMapping<Item>("factor", "1.5")).Single().Should().Be(3m);
		items.Select(sorting.GetMapping<Item>("owner", guid.ToString())).Single().Should().Be(guid);
		items.Select(sorting.GetMapping<Item>("state", "Ready")).Single().Should().Be(TestState.Ready);
	}

	[Theory]
	[InlineData("offset", "invalid")]
	[InlineData("offset", "999999999999999999999999")]
	[InlineData("owner", "invalid-guid")]
	[InlineData("state", "999")]
	[InlineData("state", "Unknown")]
	[InlineData("id", "unexpected")]
	public void ParameterizedMapping_RejectsInvalidOrUnsupportedArguments(string field, string argument)
	{
		var sorting = new QueryableSortBy()
			.Map<Item, int>("id", item => item.Id)
			.MapParameterized<Item, int, int>("offset", (item, offset) => item.Id + offset)
			.MapParameterized<Item, Guid, Guid>("owner", (item, owner) => owner)
			.MapParameterized<Item, TestState, TestState>("state", (item, state) => state);

		var action = () => sorting.GetMapping<Item>(field, argument);
		action.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void GraphQlSort_PreservesArgumentWithoutStringEncoding()
	{
		var input = new GqlSortBy { Field = "count", IsDescending = true, Argument = "Main_Warehouse" };

		new[] { input }.ToSortExpressions().Should().Equal(new SortExpression("count", true, "Main_Warehouse"));
	}

	private sealed record Item(int Id, IReadOnlyList<Stock> Stocks);
	private sealed record Stock(string Code, int Quantity);
	private enum TestState { Ready, Closed }
}
