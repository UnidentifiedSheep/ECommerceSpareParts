using Bogus;
using Main.Entities.Balance;
using Main.Enums;
using Test.Common.Abstractions;

namespace Tests.DataBuilders.Balance;

public class TransactionBuilder(Faker faker) : BuilderBase<Transaction>(faker)
{
    public Guid? SenderId { get; private set; }
    public Guid? ReceiverId { get; private set; }
    public int? CurrencyId { get; private set; }
    public TransactionType? Type { get; private set; }
    public decimal? Amount { get; private set; }
    public DateTime? TransactionDateTime { get; private set; }
    public bool CompleteTransaction { get; private set; }

    public TransactionBuilder WithSenderId(Guid senderId)
    {
        SenderId = senderId;
        return this;
    }

    public TransactionBuilder WithReceiverId(Guid receiverId)
    {
        ReceiverId = receiverId;
        return this;
    }

    public TransactionBuilder WithCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
        return this;
    }

    public TransactionBuilder WithType(TransactionType type)
    {
        Type = type;
        return this;
    }

    public TransactionBuilder WithAmount(decimal amount)
    {
        Amount = amount;
        return this;
    }

    public TransactionBuilder WithTransactionDateTime(DateTime transactionDateTime)
    {
        TransactionDateTime = transactionDateTime;
        return this;
    }

    public TransactionBuilder Completed()
    {
        CompleteTransaction = true;
        return this;
    }

    public override Transaction Build()
    {
        var transaction = Transaction.Create(
            SenderId ?? Guid.NewGuid(),
            ReceiverId ?? Guid.NewGuid(),
            CurrencyId ?? Faker.Random.Int(1, 100),
            Type ?? TransactionType.Transfer,
            Amount ?? Faker.Random.Decimal(1m, 10_000m),
            TransactionDateTime ?? DateTime.UtcNow);

        if (CompleteTransaction)
            transaction.Complete();

        return transaction;
    }
}