using Application.Handlers.Currencies.CreateCurrency;
using Exceptions.Exceptions.Currencies;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Currencies;

[Collection("Combined collection")]
public class CreateCurrencyTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    
    public CreateCurrencyTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }
    
    private string GetValidName() => Global.Faker.Lorem.Letter(10);
    private string GetValidCurrencyCode() => Global.Faker.Lorem.Letter(2);
    private string GetValidCurrencySign() => Global.Faker.Lorem.Letter();
    private string GetValidShortName() => Global.Faker.Lorem.Letter(3);

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("d")]
    [InlineData("jjjjjjjjjjjjjjjjjjjjjjjjjjjjjj")]
    public async Task CreateCurrency_WithInvalidShortName_ThrowsValidationException(string shortName)
    {
        var command = new CreateCurrencyCommand(shortName, GetValidName(), GetValidCurrencySign(), GetValidCurrencyCode());
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("d")]
    [InlineData("ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd")]
    public async Task CreateCurrency_WithInvalidName_ThrowsValidationException(string name)
    {
        var command = new CreateCurrencyCommand(GetValidShortName(), name, GetValidCurrencySign(), GetValidCurrencyCode());
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("sdfsddddd")]
    public async Task CreateCurrency_WithInvalidSign_ThrowsValidationException(string sign)
    {
        var command = new CreateCurrencyCommand(GetValidShortName(), GetValidName(), sign, GetValidCurrencyCode());
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("d")]
    [InlineData("sdfsddiimimimimiimimiiimmmmmimimiimmmiiimim")]
    public async Task CreateCurrency_WithInvalidCode_ThrowsValidationException(string code)
    {
        var command = new CreateCurrencyCommand(GetValidShortName(), GetValidName(), GetValidCurrencySign(), code);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateCurrency_WithDuplicateData_ThrowsErrors()
    {
        var shortName = GetValidShortName();
        var name = GetValidName();
        var code = GetValidCurrencyCode();
        var sign = GetValidCurrencySign();

        await _mediator.Send(new CreateCurrencyCommand(shortName, name, sign, code));
        
        await Assert.ThrowsAsync<CurrencyCodeTakenException>(async () =>
            await _mediator.Send(new CreateCurrencyCommand(GetValidShortName(), GetValidName(), GetValidCurrencySign(), code)));
        
        await Assert.ThrowsAsync<CurrencyNameTakenException>(async () =>
            await _mediator.Send(new CreateCurrencyCommand(GetValidShortName(), name, GetValidCurrencySign(), GetValidCurrencyCode())));
        
        await Assert.ThrowsAsync<CurrencySignTakenException>(async () =>
            await _mediator.Send(new CreateCurrencyCommand(GetValidShortName(), GetValidName(), sign, GetValidCurrencyCode())));
        
        await Assert.ThrowsAsync<CurrencyShortNameTakenException>(async () =>
            await _mediator.Send(new CreateCurrencyCommand(shortName, GetValidName(), GetValidCurrencySign(), GetValidCurrencyCode())));
    }

    [Fact]
    public async Task CreateCurrency_WithValidData_Succeeds()
    {
        var shortName = GetValidShortName();
        var name = GetValidName();
        var code = GetValidCurrencyCode();
        var sign = GetValidCurrencySign();

        await _mediator.Send(new CreateCurrencyCommand(shortName, name, sign, code));

        var currencyInDb = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(currencyInDb);
        Assert.Equal(shortName.Trim(), currencyInDb.ShortName);
        Assert.Equal(name.Trim(), currencyInDb.Name);
        Assert.Equal(code.Trim(), currencyInDb.Code);
        Assert.Equal(sign.Trim(), currencyInDb.CurrencySign);
    }
}