using Application.Common.Querying;
using FluentAssertions;
using GraphQL.Common.Attributes;
using GraphQL.Common.Extensions;
using GraphQL.Common.Types;
using GraphQL.Common.Types.Sorting;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Tests.GraphQl.Sorting;

public sealed class SortableByTests
{
	private const string SchemaName = "sortable-by-tests";

	static SortableByTests()
		=> QueryableSortBy.Value
			.MapDefault<SortableEntity, int>(entity => entity.Id)
			.Map<SortableEntity, int>("id", entity => entity.Id)
			.Map<SortableEntity, decimal>("approximateBalance", entity => entity.Balance)
			.MapParameterized<SortableEntity, string, int>("stockCount", (entity, storage) => entity.Id)
			.Map<SortableEntity, int>("count", entity => entity.Id)
			.MapParameterized<SortableEntity, string, int>("count", (entity, storage) => entity.Id);

	[Fact]
	public async Task Schema_GeneratesEnumFromRegisteredFieldsAndTypedSortInput()
	{
		var executor = await CreateExecutor();

		var sdl = executor.Schema.ToString();

		sdl.Should().Contain("sortBy: [SortableEntitySortBy!]");
		sdl.Should().Contain("field: SortableEntitySortField!");
		var values = executor.Schema.Types.GetType<HotChocolate.Types.EnumType>("SortableEntitySortField")
			.Values
			.Select(value => value.Name);
		values.Should().BeEquivalentTo("APPROXIMATE_BALANCE", "COUNT", "ID", "STOCK_COUNT");
	}

	[Fact]
	public async Task Schema_AppliesSchemaLevelPrefixToGeneratedSortTypes()
	{
		var executor = await CreateExecutor("Search");

		var sdl = executor.Schema.ToString();

		sdl.Should().Contain("sortBy: [SearchSortableEntitySortBy!]");
		sdl.Should().Contain("field: SearchSortableEntitySortField!");
		sdl.Should().NotContain("input SortableEntitySortBy");
		executor.Schema.Types.GetType<HotChocolate.Types.EnumType>("SearchSortableEntitySortField")
			.Values
			.Select(value => value.Name)
			.Should()
			.Contain("APPROXIMATE_BALANCE");
	}

	[Fact]
	public async Task Schema_DescribesFieldsThatAcceptArguments()
	{
		var executor = await CreateExecutor();
		var values = executor.Schema.Types.GetType<HotChocolate.Types.EnumType>("SortableEntitySortField")
			.Values
			.ToDictionary(value => value.Name, value => value.Description);

		values["STOCK_COUNT"].Should().Be("Requires argument of type String.");
		values["COUNT"].Should().Be("Accepts optional argument of type String.");
		values["ID"].Should().BeNull();
	}

	[Fact]
	public async Task Execution_MapsEnumValuesBackToRegisteredFieldNames()
	{
		var executor = await CreateExecutor();

		var result = await executor.ExecuteAsync(
			"""
			{
			  sorts(input: { sortBy: [
			    { field: APPROXIMATE_BALANCE, isDescending: true },
			    { field: COUNT, isDescending: false, argument: "main" }
			  ] })
			}
			""",
			TestContext.Current.CancellationToken);

		var operation = result.ExpectOperationResult();
		operation.Errors.Should().BeNullOrEmpty();
		result.ToJson().Should().Contain("\"sorts\": \"approximateBalance:True:|count:False:main\"");
	}

	[Fact]
	public async Task Execution_RejectsFieldsThatAreNotRegistered()
	{
		var executor = await CreateExecutor();

		var result = await executor.ExecuteAsync(
			"{ sorts(input: { sortBy: [{ field: NAME, isDescending: false }] }) }",
			TestContext.Current.CancellationToken);

		result.ExpectOperationResult().Errors.Should().NotBeNullOrEmpty();
	}

	[Theory]
	[InlineData("id", "ID")]
	[InlineData("approximateBalance", "APPROXIMATE_BALANCE")]
	[InlineData("saleContentId", "SALE_CONTENT_ID")]
	[InlineData("purchaseDatetime", "PURCHASE_DATETIME")]
	[InlineData("item2Count", "ITEM2_COUNT")]
	public void ToConstantCase_SplitsCamelCaseWords(string name, string expected)
		=> SortFieldEnumType<SortableEntity>.ToConstantCase(name).Should().Be(expected);

	private static async Task<IRequestExecutor> CreateExecutor(string? prefix = null)
	{
		var services = new ServiceCollection();
		var builder = services.AddGraphQL(SchemaName).AddQueryType<SortableQuery>();
		if (prefix is not null)
			builder.AddSortTypePrefix(prefix);
		var provider = services.BuildServiceProvider();
		return await provider.GetRequestExecutorAsync(SchemaName, TestContext.Current.CancellationToken);
	}

	public sealed record SortableEntity(int Id, decimal Balance);

	public sealed record SortableInput
	{
		[GraphQLName("sortBy")]
		[SortableBy<SortableEntity>]
		public IReadOnlyCollection<GqlSortBy>? SortBy { get; init; }
	}

	public sealed class SortableQuery
	{
		public string Sorts(SortableInput input)
			=> string.Join(
				"|",
				(input.SortBy ?? []).Select(sort => $"{sort.Field}:{sort.IsDescending}:{sort.Argument}"));
	}
}
