using Exceptions;
using FluentAssertions;
using Main.Entities.Balance;
using Main.Enums;
using Main.Enums.Balances;

namespace Tests.Domain.Balance;

public class TransactionTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var tx = Create();

        tx.Status.Should().Be(TransactionStatus.Pending);
        tx.Amount.Should().Be(100m);
    }

    [Fact]
    public void Create_SameSenderAndReceiver_Throws()
    {
        var id = Guid.NewGuid();

        var act = () => Transaction.Create(
            id,
            id,
            1,
            TransactionType.Transfer,
            100m,
            DateTime.UtcNow,
            TransactionSourceType.Manual);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void Complete_SetsCompleted()
    {
        var tx = Create();

        tx.Complete();

        tx.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Complete_Twice_Throws()
    {
        var tx = Create();

        tx.Complete();

        var act = () => tx.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Apply_Completed_TransfersMoney()
    {
        var tx = Create();

        var sender = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);

        sender.IncrementBalance(200m);

        tx.Complete();
        tx.Apply(sender, receiver);

        sender.Balance.Should().Be(100m);
        receiver.Balance.Should().Be(100m);
        tx.IsCompletionApplied.Should().BeTrue();
    }

    [Fact]
    public void Apply_Reversed_ReturnsMoneyBack()
    {
        var tx = Create();

        var sender = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);

        sender.IncrementBalance(200m);

        tx.Complete();
        tx.Apply(sender, receiver);

        tx.Reverse(Guid.NewGuid());
        tx.Apply(sender, receiver);

        sender.Balance.Should().Be(200m);
        receiver.Balance.Should().Be(0m);
    }

    [Fact]
    public void Apply_WithFinancialProfiles_ManualUserToUser_UsesBaseAmountForProfiles()
    {
        var tx = Create();
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(tx.SenderId);
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId);
        senderBalance.IncrementBalance(200m);
        senderProfile.Credit(200m);

        tx.Complete();
        tx.Apply(
            senderBalance,
            receiverBalance,
            senderProfile,
            receiverProfile,
            120m,
            Guid.NewGuid());

        senderBalance.Balance.Should().Be(100m);
        receiverBalance.Balance.Should().Be(100m);
        senderProfile.Balance.Should().Be(80m);
        receiverProfile.Balance.Should().Be(120m);
        tx.IsCompletionApplied.Should().BeTrue();
    }

    [Fact]
    public void Apply_WithFinancialProfiles_SystemSettlementReversed_RollsBackFinancialProfile()
    {
        var systemId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var tx = Transaction.Create(
            systemId,
            receiverId,
            1,
            TransactionType.Transfer,
            100m,
            DateTime.UtcNow,
            TransactionSourceType.Sale);
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(tx.SenderId, decimal.MinValue);
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId);
        receiverProfile.Credit(120m);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance, senderProfile, receiverProfile, 120m, systemId);
        tx.Reverse(systemId);
        tx.Apply(senderBalance, receiverBalance, senderProfile, receiverProfile, 120m, systemId);

        senderBalance.Balance.Should().Be(0m);
        receiverBalance.Balance.Should().Be(0m);
        receiverProfile.Balance.Should().Be(120m);
        tx.IsReversalApplied.Should().BeTrue();
    }

    [Fact]
    public void Apply_WithFinancialProfiles_SenderProfileMismatch_Throws()
    {
        var tx = Create();
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(Guid.NewGuid());
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId);

        tx.Complete();

        var act = () => tx.Apply(
            senderBalance,
            receiverBalance,
            senderProfile,
            receiverProfile,
            100m,
            Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Sender financial profile user mismatch");
    }

    [Fact]
    public void Reverse_WithoutCompletionApplied_Throws()
    {
        var tx = Create();

        var act = () => tx.Reverse(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Apply_MismatchedCurrency_Throws()
    {
        var tx = Create();

        var sender = UserBalance.Create(tx.SenderId, 999);
        var receiver = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);

        tx.Complete();

        var act = () => tx.Apply(sender, receiver);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Apply_TwiceCompleted_Throws()
    {
        var tx = Create();

        var sender = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);

        sender.IncrementBalance(200m);

        tx.Complete();
        tx.Apply(sender, receiver);

        var act = () => tx.Apply(sender, receiver);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Apply_WithoutCompletionOrReverse_Throws()
    {
        var tx = Create();

        var sender = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);

        var act = () => tx.Apply(sender, receiver);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Nothing to apply");
    }

    [Fact]
    public void Apply_ReversalAppliedTwice_Throws()
    {
        var tx = Create();

        var sender = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);

        sender.IncrementBalance(200m);

        tx.Complete();
        tx.Apply(sender, receiver);

        tx.Reverse(Guid.NewGuid());
        tx.Apply(sender, receiver);

        var act = () => tx.Apply(sender, receiver);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Reversed already applied.");
    }

    [Fact]
    public void Apply_CompletedTwice_Throws()
    {
        var tx = Create();

        var sender = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);

        sender.IncrementBalance(200m);

        tx.Complete();
        tx.Apply(sender, receiver);

        var act = () => tx.Apply(sender, receiver);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Completion already applied.");
    }

    [Fact]
    public void Reverse_Twice_Throws()
    {
        var tx = Create();

        var sender = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);

        sender.IncrementBalance(200m);

        tx.Complete();
        tx.Apply(sender, receiver);

        tx.Reverse(Guid.NewGuid());

        var act = () => tx.Reverse(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Apply_SenderMismatch_Throws()
    {
        var tx = Create();

        var sender = UserBalance.Create(Guid.NewGuid(), tx.CurrencyId);
        var receiver = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);

        tx.Complete();

        var act = () => tx.Apply(sender, receiver);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Apply_ReceiverMismatch_Throws()
    {
        var tx = Create();

        var sender = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = UserBalance.Create(Guid.NewGuid(), tx.CurrencyId);

        tx.Complete();

        var act = () => tx.Apply(sender, receiver);

        act.Should().Throw<InvalidOperationException>();
    }

    private static Transaction Create()
    {
        return Transaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            1,
            TransactionType.Transfer,
            100m,
            DateTime.UtcNow,
            TransactionSourceType.Manual);
    }
}
