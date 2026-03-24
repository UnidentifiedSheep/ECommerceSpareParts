using Analytics.Abstractions.Dtos.PurchaseFact;
using Analytics.Application.Handlers.PurchaseFacts.UpsertPurchaseFact;
using Analytics.Integration.Tests.TestContexts;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using ValidationException = FluentValidation.ValidationException;

namespace Analytics.Integration.Tests.HandlerTests.PurchaseFacts;

[Collection("Combined collection")]
public class UpsertPurchaseFactTests : IAsyncLifetime
{
    private readonly PurchaseFactTestContext _context;
    public UpsertPurchaseFactTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<PurchaseFactTestContext>();
    }
    public async Task InitializeAsync()
    {
        await _context.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Context.ClearDatabase();
    }

    [Fact]
    public async Task UpsertPurchaseFact_WithEmptyContent_ValidationFails()
    {
        var dto = Generate(_context.Currency.Id, 0);

        await FluentActions.Invoking(async () => await _context.Mediator.Send(new UpsertPurchaseFactCommand(dto)))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    private static PurchaseFactUpsertDto Generate(int currencyId, int contentCount = 5)
    {
        var contentFaker = new Faker<PurchaseContentUpsertDto>()
            .RuleFor(x => x.Id, f => f.IndexFaker + 1)
            .RuleFor(x => x.ArticleId, f => f.Random.Int(1, 1000))
            .RuleFor(x => x.Count, f => f.Random.Int(1, 50))
            .RuleFor(x => x.Price, f => decimal.Parse(f.Commerce.Price()));

        var faker = new Faker<PurchaseFactUpsertDto>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid().ToString())
            .RuleFor(x => x.CurrencyId, _ => currencyId)
            .RuleFor(x => x.SupplierId, _ => Guid.NewGuid())
            .RuleFor(x => x.Content, _ => contentFaker.Generate(contentCount));

        return faker.Generate();
    }
}