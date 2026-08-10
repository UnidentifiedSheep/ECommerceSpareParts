using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Application.Common.Services.Settings;
using Attributes;
using Domain.CommonEntities;
using FluentAssertions;
using Moq;

namespace Tests.Tests.Services.Settings;

public sealed class SettingsServiceTests
{
    [Fact]
    public async Task SetSetting_ExistingSetting_UsesApplicationTransaction()
    {
        var existing = new TestSetting(new TestSettingData(1));
        var replacement = new TestSetting(new TestSettingData(2));
        var repository = new Mock<IRepository<Setting, string>>();
        repository
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Criteria<Setting>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var transactionContext = new Mock<IApplicationTransactionContext>();
        transactionContext.SetupGet(x => x.UnitOfWork)
            .Returns(unitOfWork.Object);
        var transactionService = new Mock<IApplicationTransactionService>();
        transactionService
            .Setup(x => x.ExecuteAsync(
                It.IsAny<TransactionalAttribute?>(),
                It.IsAny<Func<IApplicationTransactionContext, CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<
                TransactionalAttribute?,
                Func<IApplicationTransactionContext, CancellationToken, Task>,
                CancellationToken>((_, action, ct) =>
                action(transactionContext.Object, ct));
        var settingsContainer = new Mock<ISettingsContainer>();
        var service = new SettingsService(
            repository.Object,
            transactionService.Object,
            settingsContainer.Object);

        await service.SetSetting(replacement);

        existing.Json.Should().Be(replacement.Json);
        transactionService.Verify(x => x.ExecuteAsync(
                It.IsAny<TransactionalAttribute?>(),
                It.IsAny<Func<IApplicationTransactionContext, CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(
            x => x.ExecuteWithTransaction(
                It.IsAny<TransactionalAttribute>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        settingsContainer.Verify(x => x.Set(replacement), Times.Once);
    }

    private sealed record TestSettingData(int Value);

    private sealed class TestSetting(TestSettingData data)
        : Setting<TestSettingData>(Name, data)
    {
        public const string Name = "test-setting";
    }
}
