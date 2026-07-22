using Main.Entities.Balance;
using Main.Entities.Organization;
using Main.Entities.User;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Balance;
using Tests.Interfaces;
using Tests.TestContexts.Currency;

namespace Tests.TestContexts.Balance;

public class BalanceTestContext(
    DContext context,
    UsersTestContext usersTestContext,
    CurrencyTestContext currencyTestContext
) : TestContextBase<DContext>(context), IDependentTestContext
{
    private readonly List<Transaction> _transactions = [];

    public IReadOnlyList<User> Users => usersTestContext.Users.ToList();
    public IReadOnlyList<Main.Entities.Currency.Currency> Currencies => currencyTestContext.Currencies;
    public IReadOnlyList<Transaction> Transactions => _transactions;

    public static Type[] DependsOn { get; } =
    [
        typeof(UsersTestContext),
        typeof(CurrencyTestContext)
    ];

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var users = Users;
        var currency = Currencies.First();

        var senderBalance = new UserBalanceBuilder(Faker)
            .WithUserId(users[0].Id)
            .WithCurrencyId(currency.Id)
            .Build();

        var receiverBalance = new UserBalanceBuilder(Faker)
            .WithUserId(users[1].Id)
            .WithCurrencyId(currency.Id)
            .Build();

        var senderProfile = OrganizationFinancialProfile.Create(users[0].Id);
        var receiverProfile = OrganizationFinancialProfile.Create(users[1].Id);
        receiverBalance.IncrementBalance(100m);

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(users[0].Id)
            .WithReceiverId(users[1].Id)
            .WithCurrencyId(currency.Id)
            .WithAmount(100m)
            .WithTransactionDateTime(DateTime.UtcNow.AddDays(-2))
            .WithBalances(senderBalance, receiverBalance)
            .Completed()
            .Applied()
            .Build();

        await DbContext.AddRangeAsync(
            [transaction, senderBalance, receiverBalance, senderProfile, receiverProfile],
            cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        _transactions.Add(transaction);
    }
}
