using Exceptions;
using FluentAssertions;
using Main.Entities.Balance;
using Main.Entities.Organization;
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

        var sender = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);

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

        var sender = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);

        sender.IncrementBalance(200m);

        tx.Complete();
        tx.Apply(sender, receiver);

        tx.Reverse(Guid.NewGuid());
        tx.Apply(sender, receiver);

        sender.Balance.Should().Be(200m);
        receiver.Balance.Should().Be(0m);
    }

    [Fact]
    public void ApplyProfile_CompletionWithinLimit_MarksProfileApplied()
    {
        var tx = Create();
        var senderBalance = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = OrganizationFinancialProfile.Create(tx.SenderId);
        var receiverProfile = OrganizationFinancialProfile.Create(tx.ReceiverId);
        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            0m,
            100m,
            100m);

        tx.IsCompletionApplied.Should().BeTrue();
        tx.IsCompletionProfileApplied.Should().BeTrue();
    }

    [Fact]
    public void ApplyProfile_ReversalBelowLimit_IsAllowed()
    {
        var tx = Create();
        var senderBalance = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = OrganizationFinancialProfile.Create(tx.SenderId);
        var receiverProfile = OrganizationFinancialProfile.Create(tx.ReceiverId);
        senderBalance.IncrementBalance(200m);
        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            200m,
            120m,
            120m);
        tx.Reverse(Guid.NewGuid());
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            decimal.MinValue,
            decimal.MinValue,
            120m);

        tx.IsReversalProfileApplied.Should().BeTrue();
    }

    [Fact]
    public void ApplyProfile_SenderProfileMismatch_Throws()
    {
        var tx = Create();
        var senderBalance = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = OrganizationFinancialProfile.Create(Guid.NewGuid());
        var receiverProfile = OrganizationFinancialProfile.Create(tx.ReceiverId);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);

        var act = () => ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            0m,
            100m,
            100m);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Sender profile user mismatch");
    }

    [Fact]
    public void ApplyProfile_CompletionProfileAppliedTwice_Throws()
    {
        var tx = Create();
        var senderBalance = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = OrganizationFinancialProfile.Create(tx.SenderId);
        var receiverProfile = OrganizationFinancialProfile.Create(tx.ReceiverId);
        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            0m,
            100m,
            100m);

        var act = () => ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            0m,
            100m,
            100m);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Completion profile already applied");
    }

    [Theory]
    [InlineData(TransactionSourceType.Purchase)]
    [InlineData(TransactionSourceType.Logistic)]
    public void ApplyProfile_SourceType_DoesNotChangeLimitSemantics(TransactionSourceType sourceType)
    {
        var senderId = Guid.NewGuid();
        var tx = Create(
            senderId,
            SystemId,
            sourceType);
        var senderBalance = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = OrganizationFinancialProfile.Create(tx.SenderId);
        var receiverProfile = OrganizationFinancialProfile.Create(tx.ReceiverId, decimal.MinValue);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            0m,
            0m,
            100m,
            true);

        tx.IsCompletionProfileApplied.Should().BeTrue();
    }

    [Fact]
    public void ApplyProfile_ProjectingBelowMinimum_Throws()
    {
        var buyerId = Guid.NewGuid();
        var tx = Create(
            SystemId,
            buyerId,
            TransactionSourceType.Sale);
        var senderBalance = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = OrganizationFinancialProfile.Create(tx.SenderId, decimal.MinValue);
        var receiverProfile = OrganizationFinancialProfile.Create(tx.ReceiverId);
        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        var act = () => ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            0m,
            50m,
            100m,
            false);

        act.Should().Throw<InvalidInputException>();
        tx.IsCompletionProfileApplied.Should().BeFalse();
    }

    [Fact]
    public void ApplyProfile_ForceDebit_AllowsProjectedBalanceBelowMinimum()
    {
        var senderId = Guid.NewGuid();
        var tx = Create(senderId, SystemId);
        var senderBalance = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = OrganizationFinancialProfile.Create(tx.SenderId);
        var receiverProfile = OrganizationFinancialProfile.Create(tx.ReceiverId, decimal.MinValue);

        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            0m,
            0m,
            100m,
            true);

        tx.IsCompletionProfileApplied.Should().BeTrue();
    }

    [Fact]
    public void ApplyProfile_AlreadyBelowMinimumButImprovingSide_IsAllowed()
    {
        var receiverId = Guid.NewGuid();
        var tx = Create(SystemId, receiverId);
        var senderBalance = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiverBalance = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);
        var senderProfile = OrganizationFinancialProfile.Create(tx.SenderId, decimal.MinValue);
        var receiverProfile = OrganizationFinancialProfile.Create(tx.ReceiverId);
        tx.Complete();
        tx.Apply(senderBalance, receiverBalance);
        ApplyProfile(
            tx,
            senderProfile,
            receiverProfile,
            -1_000m,
            100m,
            100m,
            false);

        tx.IsCompletionProfileApplied.Should().BeTrue();
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

        var sender = OrganizationBalance.Create(tx.SenderId, 999);
        var receiver = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);

        tx.Complete();

        var act = () => tx.Apply(sender, receiver);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Apply_TwiceCompleted_Throws()
    {
        var tx = Create();

        var sender = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);

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

        var sender = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);

        var act = () => tx.Apply(sender, receiver);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Nothing to apply");
    }

    [Fact]
    public void Apply_ReversalAppliedTwice_Throws()
    {
        var tx = Create();

        var sender = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);

        sender.IncrementBalance(200m);

        tx.Complete();
        tx.Apply(sender, receiver);

        tx.Reverse(Guid.NewGuid());
        tx.Apply(sender, receiver);

        var act = () => tx.Apply(sender, receiver);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Reversed already applied.");
    }

    [Fact]
    public void Apply_CompletedTwice_Throws()
    {
        var tx = Create();

        var sender = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);

        sender.IncrementBalance(200m);

        tx.Complete();
        tx.Apply(sender, receiver);

        var act = () => tx.Apply(sender, receiver);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Completion already applied.");
    }

    [Fact]
    public void Reverse_Twice_Throws()
    {
        var tx = Create();

        var sender = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);

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

        var sender = OrganizationBalance.Create(Guid.NewGuid(), tx.CurrencyId);
        var receiver = OrganizationBalance.Create(tx.ReceiverId, tx.CurrencyId);

        tx.Complete();

        var act = () => tx.Apply(sender, receiver);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Apply_ReceiverMismatch_Throws()
    {
        var tx = Create();

        var sender = OrganizationBalance.Create(tx.SenderId, tx.CurrencyId);
        var receiver = OrganizationBalance.Create(Guid.NewGuid(), tx.CurrencyId);

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
        OrganizationFinancialProfile senderProfile,
        OrganizationFinancialProfile receiverProfile,
        decimal senderBalanceInBaseCurrency,
        decimal receiverBalanceInBaseCurrency,
        decimal amountInBaseCurrency,
        bool forceDebit = false)
    {
        FinancialProfileService.Apply(
            transaction,
            senderProfile,
            receiverProfile,
            senderBalanceInBaseCurrency,
            receiverBalanceInBaseCurrency,
            amountInBaseCurrency,
            forceDebit);
    }
}
