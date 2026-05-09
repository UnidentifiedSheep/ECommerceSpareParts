using Main.Entities.Balance;
using Main.Entities.Currency;
using Main.Entities.User;
using Main.Persistence.Context;
using MediatR;
using Test.Common.Abstractions;
using Test.Common.Interfaces;
using Tests.DataBuilders.Balance;

namespace Tests.TestContexts.Balance;

public class BalanceTestContext(
    DContext context,
    UsersTestContext usersTestContext,
    CurrencyTestContext currencyTestContext
) : TestContextBase<DContext>(context), IDependentTestContext
{
    private readonly List<Transaction> _transactions = [];

    public IReadOnlyList<User> Users => usersTestContext.Users.ToList();
    public IReadOnlyList<Currency> Currencies => currencyTestContext.Currencies;
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

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(users[0].Id)
            .WithReceiverId(users[1].Id)
            .WithCurrencyId(currency.Id)
            .WithAmount(100m)
            .WithTransactionDateTime(DateTime.UtcNow.AddDays(-2))
            .Completed()
            .Build();

        var senderBalance = new UserBalanceBuilder(Faker)
            .WithUserId(transaction.SenderId)
            .WithCurrencyId(transaction.CurrencyId)
            .Build();

        var receiverBalance = new UserBalanceBuilder(Faker)
            .WithUserId(transaction.ReceiverId)
            .WithCurrencyId(transaction.CurrencyId)
            .Build();

        transaction.Apply(senderBalance, receiverBalance);

        await DbContext.AddRangeAsync([transaction, senderBalance, receiverBalance], cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        _transactions.Add(transaction);
    }
}
