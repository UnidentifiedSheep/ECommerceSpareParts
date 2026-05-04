using Main.Entities.Currency;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Tests.DataBuilders;

namespace Tests.TestContexts;

public class CurrencyTestContext(
    DContext context, 
    IMediator mediator
    ) : TestContextBase<DContext>(context, mediator)
{
    private readonly List<Currency> _currencies = [];
    public IReadOnlyList<Currency> Currencies => _currencies;
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var created = await new CurrencyBuilder(Faker)
            .BuildManyAndAddToDb(DbContext, 3);
        
        _currencies.AddRange(created);
    }
}