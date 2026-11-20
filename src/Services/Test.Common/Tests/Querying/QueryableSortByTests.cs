using Application.Common.Extensions;
using Application.Common.Querying;
using Exceptions;
using FluentAssertions;
using GraphQL.Common.Extensions;
using GraphQL.Common.Types;

namespace Tests.Tests.Querying;

public sealed class QueryableSortByTests
{
	[Theory]
	[InlineData(
		"count",
		"count",
		false,
		null)]
	[InlineData(
		"count_desc",
		"count",
		true,
		null)]
	[InlineData(
		"count_asc",
		"count",
		false,
		null)]
	[InlineData(
		" COUNT_Main_Warehouse_DESC ",
		"count",
		true,
		"Main_Warehouse")]
	[InlineData(
		"count_MainWarehouse_asc",
		"count",
		false,
		"MainWarehouse")]
	[InlineData(
		"count_склад-1_desc",
		"count",
		true,
		"склад-1")]
	public void Parse_PreservesArgumentAndReadsDirection(
		string text,
		string field,
		bool descending,
		string? argument)
		=> SortExpression.Parse(text)
			.Should()
			.Be(
				new SortExpression(
					field,
					descending,
					argument));

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

	[Theory]
	[InlineData("count_down")]
	[InlineData("name_down")]
	[InlineData("count__desc")]
	public void TextInput_InvalidFormat_ThrowsLocalizedInvalidInputException(string text)
	{
		var action = () => new[] { text }.ToSortExpressions();
		action.Should().Throw<InvalidInputException>();
	}

	[Fact]
	public void ParameterizedMapping_BindsInsideNestedLambdaAndKeepsOrdinaryMapping()
	{
		var sorting = new QueryableSortBy()
			.Map<Item, long>("count", item => item.Stocks.Sum(stock => (long)stock.Quantity))
			.MapParameterized<Item, string, long>(
				"count",
				(item, storage) => item.Stocks
					.Where(stock => stock.Code == storage)
					.Sum(stock => (long)stock.Quantity));
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
	public void Descriptors_KeepOriginalNameAndMergeRegularAndParameterizedMappings()
	{
		var sorting = new QueryableSortBy()
			.MapDefault<Item, int>(item => item.Id, true)
			.Map<Item, long>("stockCount", item => item.Stocks.Sum(stock => (long)stock.Quantity))
			.MapParameterized<Item, string, long>(
				"stockCount",
				(item, storage) => item.Stocks.Where(stock => stock.Code == storage).Sum(stock => (long)stock.Quantity))
			.MapParameterized<Item, Guid, bool>("ownerMatch", (item, owner) => owner == Guid.Empty);

		var entity = sorting.FindEntity(typeof(Item))!;

		entity.Fields.Select(field => field.Name).Should().BeEquivalentTo("stockCount", "ownerMatch");
		var stockCount = entity.FindField("STOCKCOUNT")!;
		stockCount.Name.Should().Be("stockCount");
		stockCount.KeyType.Should().Be<long>();
		stockCount.ArgumentType.Should().Be<string>();
		stockCount.AcceptsArgument.Should().BeTrue();
		stockCount.RequiresArgument.Should().BeFalse();
		entity.FindField("ownerMatch")!.RequiresArgument.Should().BeTrue();
		entity.DefaultSelector.Should().NotBeNull();
		entity.DefaultDescending.Should().BeTrue();
	}

	[Fact]
	public void GetMapping_UsesDefaultForEmptyFieldAndRequiresArgumentForParameterizedOnlyField()
	{
		var sorting = new QueryableSortBy()
			.MapDefault<Item, int>(item => item.Id)
			.MapParameterized<Item, Guid, bool>("ownerMatch", (item, owner) => owner == Guid.Empty);
		var items = new[] { new Item(7, []) };

		items.Select(sorting.GetMapping<Item>(string.Empty).Compile()).Single().Should().Be(7);
		var withoutArgument = () => sorting.GetMapping<Item>("ownerMatch");
		var unknown = () => sorting.GetMapping<Item>("missing");
		var unregistered = () => sorting.GetMapping<Stock>("id");

		withoutArgument.Should().Throw<ArgumentException>();
		unknown.Should().Throw<ArgumentException>();
		unregistered.Should().Throw<ArgumentException>();
		sorting.GetFields(typeof(Stock)).Should().BeEmpty();
	}

	[Fact]
	public void Map_RejectsFieldsDifferingOnlyByCase()
	{
		var sorting = new QueryableSortBy().Map<Item, int>("id", item => item.Id);

		var caseConflict = () => sorting.Map<Item, int>("ID", item => item.Id);
		var remap = () => sorting.Map<Item, int>("id", item => item.Id + 1);

		caseConflict.Should().Throw<InvalidOperationException>();
		remap.Should().NotThrow();
	}

	[Fact]
	public void Descriptors_AllowDifferentKeyTypesForRegularAndParameterizedMappings()
	{
		var sorting = new QueryableSortBy()
			.MapParameterized<Item, string, int>("stock", (item, storage) => item.Id)
			.Map<Item, long?>("stock", item => item.Id);

		var field = sorting.FindEntity(typeof(Item))!.FindField("stock")!;

		field.KeyType.Should().Be<long?>();
		field.AcceptsArgument.Should().BeTrue();
		field.RequiresArgument.Should().BeFalse();
	}

	[Fact]
	public void GraphQlSort_PreservesArgumentWithoutStringEncoding()
	{
		var input = new GqlSortBy { Field = "count", IsDescending = true, Argument = "Main_Warehouse" };

		new[] { input }.ToSortExpressions()
			.Should()
			.Equal(
				new SortExpression(
					"count",
					true,
					"Main_Warehouse"));
	}

	private sealed record Item(int Id, IReadOnlyList<Stock> Stocks);

	private sealed record Stock(string Code, int Quantity);

	private enum TestState { Ready, Closed }
}
