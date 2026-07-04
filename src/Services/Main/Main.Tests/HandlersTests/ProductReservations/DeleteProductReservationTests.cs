using FluentAssertions;
using Main.Application.Handlers.ProductReservations.DeleteProductReservation;
using Main.Entities.Exceptions;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.ProductReservations;

public class DeleteProductReservationTests : IntegrationTest
{
    public DeleteProductReservationTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<StorageContentReservationTestContext>();
    }

    public StorageContentReservationTestContext TestContext =>
        GetContext<StorageContentReservationTestContext>();

    [Fact]
    public async Task WhenInvalidId_ThrowsReservationNotFoundException()
    {
        var command = new DeleteProductReservationCommand(9999);
        var act = () => Mediator.Send(command);

        await act.Should().ThrowAsync<ReservationNotFoundException>();
    }

    [Fact]
    public async Task WhenNotCancelled_CancelsReservation()
    {
        var command = new DeleteProductReservationCommand(TestContext.ActiveReservations[0].Id);
        var act = () => Mediator.Send(command);

        await act.Should().NotThrowAsync();

        var dbValue = await Context.StorageContentReservations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.ReservationId);

        dbValue.Should().NotBeNull();
        dbValue.Status.Should().Be(StorageContentReservationStatus.Canceled);
    }

    [Fact]
    public async Task WhenCancelled_DoesntThrow()
    {
        var command = new DeleteProductReservationCommand(
            TestContext.CanceledReservation.Id);
        var act = () => Mediator.Send(command);

        await act.Should().NotThrowAsync();

        var dbValue = await Context.StorageContentReservations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.ReservationId);

        dbValue.Should().NotBeNull();
        dbValue.Status.Should().Be(StorageContentReservationStatus.Canceled);
    }
}