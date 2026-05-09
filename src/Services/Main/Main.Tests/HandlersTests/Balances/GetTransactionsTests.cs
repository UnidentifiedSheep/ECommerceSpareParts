using Abstractions.Models;
using FluentAssertions;
using Main.Application.Handlers.Balance.GetTransactions;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Balance;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Balances;

public class GetTransactionsTests : IntegrationTest
{
    public GetTransactionsTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<BalanceTestContext>();
    }

    private BalanceTestContext TestContext => GetContext<BalanceTestContext>();

    [Fact]
    public async Task GetTransactions_BySender_ReturnsTransactions()
    {
        var senderId = TestContext.Users[0].Id;

        var result = await Mediator.Send(GetQuery(senderId: senderId));

        result.Transactions.Should().ContainSingle();
        result.Transactions[0].SenderId.Should().Be(senderId);
    }

    [Fact]
    public async Task GetTransactions_ByReceiver_ReturnsTransactions()
    {
        var receiverId = TestContext.Users[1].Id;

        var result = await Mediator.Send(GetQuery(receiverId: receiverId));

        result.Transactions.Should().ContainSingle();
        result.Transactions[0].ReceiverId.Should().Be(receiverId);
    }

    [Fact]
    public async Task GetTransactions_ByCurrency_ReturnsOnlyCurrencyTransactions()
    {
        var currencyId = TestContext.Currencies[0].Id;
        var senderId = TestContext.Users[0].Id;

        var result = await Mediator.Send(GetQuery(senderId: senderId, currencyId: currencyId));

        result.Transactions.Should().OnlyContain(x => x.CurrencyId == currencyId);
    }

    [Fact]
    public async Task GetTransactions_WithSizeLimit_ReturnsLimitedPage()
    {
        var receiverId = TestContext.Users[1].Id;

        var result = await Mediator.Send(GetQuery(receiverId: receiverId, size: 1));

        result.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTransactions_SameSenderAndReceiver_ThrowsValidationException()
    {
        var userId = TestContext.Users[0].Id;

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(GetQuery(senderId: userId, receiverId: userId)));
    }

    [Fact]
    public async Task GetTransactions_WithoutSenderAndReceiver_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(GetQuery(senderId: null, receiverId: null)));
    }

    [Fact]
    public async Task GetTransactions_InvalidRange_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(GetQuery(
                rangeStart: DateTime.UtcNow,
                rangeEnd: DateTime.UtcNow.AddDays(-1))));
    }

    private GetTransactionsQuery GetQuery(
        Guid? senderId = null,
        Guid? receiverId = null,
        int? currencyId = null,
        int size = 20,
        DateTime? rangeStart = null,
        DateTime? rangeEnd = null)
    {
        return new GetTransactionsQuery(
            rangeStart ?? DateTime.UtcNow.AddDays(-10),
            rangeEnd ?? DateTime.UtcNow.AddDays(1),
            currencyId,
            senderId,
            receiverId,
            new Cursor<(Guid id, DateTime dt)>((Guid.Empty, DateTime.MinValue), size));
    }
}
