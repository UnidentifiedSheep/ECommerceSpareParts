using Exceptions;
using FluentAssertions;
using Main.Entities.Balance;
using Main.Enums;
using Main.Enums.Balances;

namespace Tests.Domain.Balance;

public class TransactionTests
{
    private static readonly Guid SystemId = Guid.NewGuid();
    private static readonly ITransactionFinancialProfileService FinancialProfileService =
        new TransactionFinancialProfileService();

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

        sender.Balance.Should().Be(300m);
        receiver.Balance.Should().Be(-100m);
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
    public void ApplyProfile_CompletionApplied_UsesBaseAmountForProfiles()
    {
        var tx = Create();
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(tx.SenderId);
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId);
        senderBalance.IncrementBalance(200m);
        senderProfile.Credit(200m);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(tx, senderProfile, receiverProfile, 120m, SystemId);

        senderBalance.Balance.Should().Be(300m);
        receiverBalance.Balance.Should().Be(-100m);
        senderProfile.Balance.Should().Be(80m);
        receiverProfile.Balance.Should().Be(120m);
        tx.IsCompletionApplied.Should().BeTrue();
        tx.IsCompletionProfileApplied.Should().BeTrue();
    }

    [Fact]
    public void ApplyProfile_ReversalAfterCompletionProfileApplied_RollsBackProfile()
    {
        var tx = Create();
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(tx.SenderId);
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId);
        senderBalance.IncrementBalance(200m);
        senderProfile.Credit(200m);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(tx, senderProfile, receiverProfile, 120m, SystemId);
        tx.Reverse(Guid.NewGuid());
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(tx, senderProfile, receiverProfile, 120m, SystemId);

        senderProfile.Balance.Should().Be(200m);
        receiverProfile.Balance.Should().Be(0m);
        tx.IsReversalProfileApplied.Should().BeTrue();
    }

    [Fact]
    public void ApplyProfile_SenderProfileMismatch_Throws()
    {
        var tx = Create();
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(Guid.NewGuid());
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);

        var act = () => ApplyProfile(tx, senderProfile, receiverProfile, 100m, SystemId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Sender profile user mismatch");
    }

    [Fact]
    public void ApplyProfile_CompletionProfileAppliedTwice_Throws()
    {
        var tx = Create();
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(tx.SenderId);
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId);
        senderProfile.Credit(200m);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(tx, senderProfile, receiverProfile, 100m, SystemId);

        var act = () => ApplyProfile(tx, senderProfile, receiverProfile, 100m, SystemId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Completion profile already applied");
    }

    [Theory]
    [InlineData(TransactionSourceType.Purchase)]
    [InlineData(TransactionSourceType.Logistic)]
    public void ApplyProfile_SystemOwesUser_CreditsSenderProfile(TransactionSourceType sourceType)
    {
        var senderId = Guid.NewGuid();
        var tx = Create(senderId, SystemId, sourceType);
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(tx.SenderId);
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId, decimal.MinValue);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(tx, senderProfile, receiverProfile, 100m, SystemId);

        senderProfile.Balance.Should().Be(100m);
        receiverProfile.Balance.Should().Be(0m);
    }

    [Fact]
    public void ApplyProfile_Sale_DebitsBuyerProfile()
    {
        var buyerId = Guid.NewGuid();
        var tx = Create(SystemId, buyerId, TransactionSourceType.Sale);
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(tx.SenderId, decimal.MinValue);
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId);
        receiverProfile.Credit(100m);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(tx, senderProfile, receiverProfile, 100m, SystemId);

        senderProfile.Balance.Should().Be(0m);
        receiverProfile.Balance.Should().Be(0m);
    }

    [Fact]
    public void ApplyProfile_ManualUserToSystem_CreditsUserProfile()
    {
        var senderId = Guid.NewGuid();
        var tx = Create(senderId, SystemId, TransactionSourceType.Manual);
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(tx.SenderId);
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId, decimal.MinValue);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(tx, senderProfile, receiverProfile, 100m, SystemId);

        senderProfile.Balance.Should().Be(100m);
        receiverProfile.Balance.Should().Be(0m);
    }

    [Fact]
    public void ApplyProfile_ManualSystemToUser_DebitsUserProfile()
    {
        var receiverId = Guid.NewGuid();
        var tx = Create(SystemId, receiverId, TransactionSourceType.Manual);
        var senderBalance = UserBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = UserBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = UserFinancialProfile.Create(tx.SenderId, decimal.MinValue);
        var receiverProfile = UserFinancialProfile.Create(tx.ReceiverId);
        receiverProfile.Credit(100m);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(tx, senderProfile, receiverProfile, 100m, SystemId);

        senderProfile.Balance.Should().Be(0m);
        receiverProfile.Balance.Should().Be(0m);
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

    private static Transaction Create(
        Guid? senderId = null,
        Guid? receiverId = null,
        TransactionSourceType sourceType = TransactionSourceType.Manual)
    {
        return Transaction.Create(
            senderId ?? Guid.NewGuid(),
            receiverId ?? Guid.NewGuid(),
            1,
            TransactionType.Transfer,
            100m,
            DateTime.UtcNow,
            sourceType);
    }

    private static void ApplyProfile(
        Transaction transaction,
        UserFinancialProfile senderProfile,
        UserFinancialProfile receiverProfile,
        decimal amountInBaseCurrency,
        Guid systemId,
        bool forceDebit = false)
    {
        FinancialProfileService.Apply(
            transaction,
            senderProfile,
            receiverProfile,
            amountInBaseCurrency,
            systemId,
            forceDebit);
    }
}
