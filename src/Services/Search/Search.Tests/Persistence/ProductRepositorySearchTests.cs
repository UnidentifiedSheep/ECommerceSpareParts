using System.Text;
using Abstractions.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using OpenSearch.Client;
using OpenSearch.Net;
using Search.Abstractions.Options;
using Search.Application.Models.CatalogueSearch;
using Search.Entities;
using Search.Enums;
using Search.Persistence;
using Search.Persistence.Interfaces;

namespace Search.Tests.Persistence;

public sealed class ProductRepositorySearchTests
{
    [Fact]
    public async Task Search_ShouldBuildRequestedSkuAndNameModes()
    {
        var (repository, requestBody) = CreateRepository();
        var criteria = CreateCriteria(
            "bosch123",
            new HashSet<SearchMatchType> { SearchMatchType.Exact },
            new HashSet<SearchMatchType> { SearchMatchType.Contains });

        var result = await repository.Search(criteria);

        result.Total.Should().Be(0);
        requestBody().Should().Contain("\"normalizedSku\"");
        requestBody().Should().Contain("\"name.contains\"");
        requestBody().Should().NotContain("\"name.prefix\"");
    }

    [Fact]
    public async Task Search_WhenShortQueryHasOnlyFuzzyModes_ShouldUseMatchNone()
    {
        var (repository, requestBody) = CreateRepository();
        var criteria = CreateCriteria(
            "abc",
            new HashSet<SearchMatchType> { SearchMatchType.Fuzzy },
            new HashSet<SearchMatchType> { SearchMatchType.Fuzzy });

        await repository.Search(criteria);

        requestBody().Should().Contain("\"match_none\"");
        requestBody().Should().NotContain("\"fuzzy\"");
    }

    private static CatalogueSearchCriteria CreateCriteria(
        string query,
        IReadOnlySet<SearchMatchType> skuModes,
        IReadOnlySet<SearchMatchType> nameModes)
    {
        return new CatalogueSearchCriteria
        {
            Query = query,
            SkuModes = skuModes,
            NameModes = nameModes,
            ProducerIds = [],
            Pagination = new Pagination(0, 20)
        };
    }

    private static (ProductRepository Repository, Func<string> RequestBody) CreateRepository()
    {
        var response = Encoding.UTF8.GetBytes(
            "{\"hits\":{\"total\":{\"value\":0,\"relation\":\"eq\"},\"hits\":[]}}");
        string requestBody = string.Empty;
        var connection = new InMemoryConnection(response, 200);
        var settings = new ConnectionSettings(
                new SingleNodeConnectionPool(new Uri("http://localhost:9200")),
                connection)
            .DisableDirectStreaming()
            .OnRequestCompleted(details =>
            {
                if (details.RequestBodyInBytes is not null)
                    requestBody = Encoding.UTF8.GetString(details.RequestBodyInBytes);
            });
        var client = new OpenSearchClient(settings);
        var options = new Mock<IOptionsMonitor<OpenSearchOptions>>();
        options.SetupGet(x => x.CurrentValue)
            .Returns(new OpenSearchOptions
            {
                Uri = "http://localhost:9200",
                IndexOptions = new OpenSearchIndexOptions
                {
                    Products = "products-v2",
                    Producers = "producers-v1",
                    CatalogueCandidates = "catalogue-candidates-v2"
                }
            });
        var initializer = new Mock<IIndexInitializer<Product>>();
        initializer
            .Setup(x => x.LazyInitialize(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return (
            new ProductRepository(options.Object, client, initializer.Object),
            () => requestBody);
    }
}
