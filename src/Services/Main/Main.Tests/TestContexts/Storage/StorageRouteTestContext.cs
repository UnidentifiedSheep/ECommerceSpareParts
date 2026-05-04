using Main.Entities.Storage;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Extensions;
using Test.Common.Interfaces;
using Tests.DataBuilders.Storage;

namespace Tests.TestContexts;

public class StorageRouteTestContext(
    DContext ctx,
    IMediator mediator,
    StorageTestContext storageTestContext,
    CurrencyTestContext currencyTestContext,
    UsersTestContext usersTestContext)
    : TestContextBase<DContext>(ctx, mediator), ITestContextRegistrator
{
    public StorageRoute ActiveRoute { get; private set; } = null!;
    public StorageRoute UnactiveRoute { get; private set; } = null!;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var storages = storageTestContext.Storages.ToList();
        var currencyId = currencyTestContext.Currencies[0].Id;

        var from = storages.First().Name;
        var to = storages.Last().Name;

        var builders = new List<StorageRouteBuilder>
        {
            new StorageRouteBuilder(Faker)
                .WithFrom(from)
                .WithTo(to)
                .WithCurrencyId(currencyId)
                .WithCarrierId(usersTestContext.Users.First().Id)
                .Active(),

            new StorageRouteBuilder(Faker)
                .WithFrom(to)
                .WithTo(from)
                .WithCurrencyId(currencyId)
                .WithCarrierId(usersTestContext.Users.Last().Id)
                .Inactive()
        };

        var routes = await builders.BuildManyCombinedAndAddToDb(DbContext, 1);
        ActiveRoute = routes.First(x => x.IsActive);
        UnactiveRoute = routes.First(x => !x.IsActive);
    }

    public static void Register(ITest test)
    {
        test.RegisterBasicContext<StorageTestContext>();
        test.RegisterBasicContext<CurrencyTestContext>();
        test.RegisterBasicContext<UsersTestContext>();
        test.RegisterBasicContext<StorageRouteTestContext>();
    }
}