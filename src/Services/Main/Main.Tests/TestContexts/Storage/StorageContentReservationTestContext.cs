using Main.Entities.Storage;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Storage;
using Tests.Interfaces;
using Tests.TestContexts.Currency;

namespace Tests.TestContexts.Storage;

public class StorageContentReservationTestContext(
    DContext context,
    UsersTestContext usersTestContext,
    ProductTestContext productTestContext,
    CurrencyTestContext currencyTestContext
)
    : TestContextBase<DContext>(context), IDependentTestContext
{
    private readonly List<StorageContentReservation> _activeReservations = [];
    private readonly List<StorageContentReservation> _reservations = [];

    public IReadOnlyList<StorageContentReservation> Reservations => _reservations;
    public IReadOnlyList<StorageContentReservation> ActiveReservations => _activeReservations;
    public StorageContentReservation LockedReservation { get; private set; } = null!;
    public StorageContentReservation DoneReservation { get; private set; } = null!;
    public StorageContentReservation CanceledReservation { get; private set; } = null!;

    public static Type[] DependsOn { get; } =
    [
        typeof(UsersTestContext),
        typeof(ProductTestContext),
        typeof(CurrencyTestContext)
    ];

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var users = usersTestContext.Users.ToList();
        var products = productTestContext.Products;
        var currencyId = currencyTestContext.Currencies[0].Id;

        _activeReservations.AddRange(
            new StorageContentReservationBuilder(Faker)
                .WithUsers(users)
                .WithProducts(products)
                .WithReservedCount(Faker.Random.Int(1, 10))
                .WithProposedPrice(100m, currencyId)
                .WithComment("reservation")
                .BuildMany(3));

        LockedReservation = new StorageContentReservationBuilder(Faker)
            .WithUserId(users[0].Id)
            .WithProductId(products[3 % products.Count].Id)
            .WithReservedCount(3)
            .WithCurrentCount(1)
            .Build();

        DoneReservation = new StorageContentReservationBuilder(Faker)
            .WithUserId(users[1 % users.Count].Id)
            .WithProductId(products[4 % products.Count].Id)
            .WithReservedCount(3)
            .WithCurrentCount(3)
            .Build();

        CanceledReservation = new StorageContentReservationBuilder(Faker)
            .WithUserId(users[2 % users.Count].Id)
            .WithProductId(products[5 % products.Count].Id)
            .WithReservedCount(3)
            .Build();
        CanceledReservation.Cancel();

        _reservations.AddRange(_activeReservations);
        _reservations.AddRange([LockedReservation, DoneReservation, CanceledReservation]);

        await DbContext.AddRangeAsync(_reservations, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}