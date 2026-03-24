using Analytics.Abstractions.Dtos.PurchaseFact;
using Analytics.Application.Handlers.PurchaseFacts.UpsertPurchaseFact;
using Analytics.Entities;
using Analytics.Integration.Tests.TestContexts;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
        var dto = Generate(0);

        var act = () => _context.Mediator.Send(new UpsertPurchaseFactCommand(dto));
        await act.Should().ThrowAsync<ValidationException>();
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(0.001)]
    [InlineData(-32423424.234)]
    [InlineData(123.4433)]
    public async Task UpsertPurchaseFact_WithInvalidPrice_ValidationFails(decimal invalidPrice)
    {
        var dto = Generate();
        dto = dto with
        {
            Content = dto.Content.ReplaceAt(0, c => c with { Price = invalidPrice })
        };
        
        var act = () => _context.Mediator.Send(new UpsertPurchaseFactCommand(dto));
        await act.Should().ThrowAsync<ValidationException>();
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UpsertPurchaseFact_WithInvalidCount_ValidationFails(int invalidCount)
    {
        var dto = Generate();
        dto = dto with
        {
            Content = dto.Content.ReplaceAt(0, c => c with { Count = invalidCount })
        };
        
        var act = () => _context.Mediator.Send(new UpsertPurchaseFactCommand(dto));
        await act.Should().ThrowAsync<ValidationException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpsertPurchaseFact_WithInvalidId_ValidationFails(string invalidId)
    {
        var dto = Generate() with { Id = invalidId };
        
        var act = () => _context.Mediator.Send(new UpsertPurchaseFactCommand(dto));
        await act.Should().ThrowAsync<ValidationException>();
    }
    
    [Fact]
    public async Task UpsertPurchaseFact_WithValidData_Succeeds()
    {
        var dto = Generate();
        
        var act = () => _context.Mediator.Send(new UpsertPurchaseFactCommand(dto));
        await act.Should().NotThrowAsync();

        var facts = await GetAllFacts();
        
        facts.Should().HaveCount(1);
        
        var fact = facts.First();
        ValidateFields(dto, fact);
    }

    [Fact]
    public async Task UpsertPurchaseFact_WithOldUpdateAtValue_DoesNothing()
    {
        var initDto = Generate();
        var dto = initDto;
        await _context.Mediator.Send(new UpsertPurchaseFactCommand(initDto));

        dto = dto with
        {
            LastUpdatedAt = DateTime.UtcNow - TimeSpan.FromMinutes(10),
            Content = dto.Content.ReplaceAt(0, x => x with{ Count = 1999})
        };

        var act = () => _context.Mediator.Send(new UpsertPurchaseFactCommand(dto));
        await act.Should().NotThrowAsync();
        
        var facts = await GetAllFacts();
        
        facts.Should().HaveCount(1);
        
        var fact = facts.First();
        ValidateFields(initDto, fact);
    }

    private async Task<List<PurchasesFact>> GetAllFacts()
    {
        return await _context.Context
            .PurchasesFacts
            .AsNoTracking()
            .Include(x => x.PurchaseContents)
            .ToListAsync();
    }

    private void ValidateFields(PurchaseFactUpsertDto dto, PurchasesFact fact)
    {
        var totalSum = dto.Content.Sum(x => x.Price * x.Count);
        
        fact.CurrencyId.Should().Be(dto.CurrencyId);
        fact.Id.Should().Be(dto.Id).And.NotBeNullOrEmpty();
        fact.SupplierId.Should().Be(dto.SupplierId);
        fact.TotalSum.Should().Be(totalSum);
        
        fact.PurchaseContents.Should().HaveSameCount(dto.Content);

        foreach (var factContent in fact.PurchaseContents)
        {
            var dtoContent = dto.Content.FirstOrDefault(x => x.Id == factContent.Id);
            dtoContent.Should().NotBeNull();
            
            factContent.Id.Should().Be(dtoContent.Id);
            factContent.Price.Should().Be(dtoContent.Price);
            factContent.Count.Should().Be(dtoContent.Count);
            factContent.ArticleId.Should().Be(dtoContent.ArticleId);
        }
    }

    private PurchaseFactUpsertDto Generate(int contentCount = 5)
    {
        var contentFaker = new Faker<PurchaseContentUpsertDto>()
            .RuleFor(x => x.Id, f => f.IndexFaker + 1)
            .RuleFor(x => x.ArticleId, f => f.Random.Int(1, 1000))
            .RuleFor(x => x.Count, f => f.Random.Int(1, 50))
            .RuleFor(x => x.Price, f => decimal.Parse(f.Commerce.Price()));

        var faker = new Faker<PurchaseFactUpsertDto>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid().ToString())
            .RuleFor(x => x.CurrencyId, _ => _context.Currency.Id)
            .RuleFor(x => x.SupplierId, _ => Guid.NewGuid())
            .RuleFor(x => x.Content, _ => contentFaker.Generate(contentCount));

        return faker.Generate();
    }
}