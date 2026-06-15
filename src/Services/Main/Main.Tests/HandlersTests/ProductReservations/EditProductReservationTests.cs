using Exceptions;
using FluentAssertions;
using Main.Application.Dtos.Product.Reservation;
using Main.Application.Handlers.ProductReservations.EditProductReservation;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.ProductReservations;

public class EditProductReservationTests : IntegrationTest
{
    public EditProductReservationTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<StorageContentReservationTestContext>();
    }

    private StorageContentReservationTestContext ReservationContext =>
        GetContext<StorageContentReservationTestContext>();

    private CurrencyTestContext CurrencyContext => GetContext<CurrencyTestContext>();

    [Fact]
    public async Task WithValidData_UpdatesReservation()
    {
        var reservation = ReservationContext.ActiveReservations[0];
        var oldComment = reservation.Comment;
        var oldProposedPrice = reservation.ProposedPrice;
        var oldProposedCurrencyId = reservation.ProposedCurrencyId;
        var currency = CurrencyContext.Currencies.Last();
        var command = new EditProductReservationCommand(
            reservation.Id,
            new EditProductReservationDto
            {
                GivenPrice = 250m,
                GivenCurrencyId = currency.Id,
                Comment = " updated comment "
            });

        await Mediator.Send(command);

        var db = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);

        db.ProposedPrice.Should().Be(250m);
        db.ProposedCurrencyId.Should().Be(currency.Id);
        db.Comment.Should().Be("updated comment");
        db.Status.Should().Be(StorageContentReservationStatus.Active);

        var @event = await Context.Events
            .OfType<ReservationManualChangeEvent>()
            .AsNoTracking()
            .SingleAsync(x => x.ReservationId == reservation.Id);

        @event.Data.Comment.Should().Be(oldComment);
        @event.Data.ProposePrice.Should().Be(oldProposedPrice);
        @event.Data.ProposedCurrencyId.Should().Be(oldProposedCurrencyId);
    }

    [Fact]
    public async Task WithNullPriceAndCurrency_ClearsProposedPrice()
    {
        var reservation = ReservationContext.ActiveReservations[0];
        var command = new EditProductReservationCommand(
            reservation.Id,
            new EditProductReservationDto
            {
                GivenPrice = null,
                GivenCurrencyId = null,
                Comment = reservation.Comment
            });

        await Mediator.Send(command);

        var db = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);

        db.ProposedPrice.Should().BeNull();
        db.ProposedCurrencyId.Should().BeNull();
    }

    [Fact]
    public async Task WithLockedReservation_UpdatesProposedPrice()
    {
        var reservation = ReservationContext.LockedReservation;
        var currency = CurrencyContext.Currencies.Last();
        var command = new EditProductReservationCommand(
            reservation.Id,
            new EditProductReservationDto
            {
                GivenPrice = 300m,
                GivenCurrencyId = currency.Id,
                Comment = "locked comment"
            });

        await Mediator.Send(command);

        var db = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);

        db.Status.Should().Be(StorageContentReservationStatus.Locked);
        db.ProposedPrice.Should().Be(300m);
        db.ProposedCurrencyId.Should().Be(currency.Id);
        db.Comment.Should().Be("locked comment");
    }

    [Fact]
    public async Task WithInvalidReservationId_ThrowsReservationNotFoundException()
    {
        var command = new EditProductReservationCommand(
            999999,
            ValidDto());

        await Assert.ThrowsAsync<ReservationNotFoundException>(() => Mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WithInvalidPrice_ThrowsValidationException(decimal price)
    {
        var command = new EditProductReservationCommand(
            ReservationContext.ActiveReservations[0].Id,
            ValidDto() with
            {
                GivenPrice = price,
                GivenCurrencyId = CurrencyContext.Currencies[0].Id
            });

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithMissingCurrency_ThrowsDbValidationException()
    {
        var command = new EditProductReservationCommand(
            ReservationContext.ActiveReservations[0].Id,
            ValidDto() with
            {
                GivenCurrencyId = 999999
            });

        await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithPriceWithoutCurrency_ThrowsInvalidInputException()
    {
        var command = new EditProductReservationCommand(
            ReservationContext.ActiveReservations[0].Id,
            ValidDto() with
            {
                GivenPrice = 100m,
                GivenCurrencyId = null
            });

        await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithCurrencyWithoutPrice_ThrowsInvalidInputException()
    {
        var command = new EditProductReservationCommand(
            ReservationContext.ActiveReservations[0].Id,
            ValidDto() with
            {
                GivenPrice = null,
                GivenCurrencyId = CurrencyContext.Currencies.First().Id
            });

        await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithDoneReservation_ThrowsInvalidInputException()
    {
        var reservation = ReservationContext.DoneReservation;
        var oldValue = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);
        var command = new EditProductReservationCommand(reservation.Id, ValidDto());

        await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));

        var db = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);
        db.Status.Should().Be(StorageContentReservationStatus.Done);
        db.Comment.Should().Be(oldValue.Comment);
        db.ProposedPrice.Should().Be(oldValue.ProposedPrice);
    }

    [Fact]
    public async Task WithCanceledReservation_ThrowsInvalidInputException()
    {
        var reservation = ReservationContext.CanceledReservation;
        var oldValue = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);
        var command = new EditProductReservationCommand(reservation.Id, ValidDto());

        await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));

        var db = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);
        db.Status.Should().Be(StorageContentReservationStatus.Canceled);
        db.Comment.Should().Be(oldValue.Comment);
        db.ProposedPrice.Should().Be(oldValue.ProposedPrice);
    }

    private EditProductReservationDto ValidDto()
    {
        return new EditProductReservationDto
        {
            GivenPrice = 200m,
            GivenCurrencyId = CurrencyContext.Currencies[0].Id,
            Comment = "valid"
        };
    }
}
