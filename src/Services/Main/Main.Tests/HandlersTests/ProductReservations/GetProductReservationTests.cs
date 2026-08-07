using FluentAssertions;
using Main.Application.Handlers.ProductReservations.GetProductReservation;
using Main.Entities.Exceptions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.ProductReservations;

public class GetProductReservationTests : IntegrationTest
{
    public GetProductReservationTests(CombinedContainerFixture fixture)
        : base(fixture)
    {
        RegisterBasicContext<ProductReservationTestContext>();
    }

    private ProductReservationTestContext TestContext =>
        GetContext<ProductReservationTestContext>();

    [Fact]
    public async Task ExistingReservation_ReturnsProjectedReservation()
    {
        var reservation = TestContext.ActiveReservations.First();

        var result = await Mediator.Send(
            new GetProductReservationQuery(reservation.Id));

        result.Reservation.Id.Should().Be(reservation.Id);
        result.Reservation.Organization.Id.Should().Be(reservation.OrganizationId);
        result.Reservation.ReservedCount.Should().Be(reservation.ReservedCount);
        result.Reservation.CurrentCount.Should().Be(reservation.CurrentCount);
        result.Reservation.ProposedPrice.Should().Be(reservation.ProposedPrice);
        result.Reservation.ProposedCurrencyId.Should().Be(reservation.ProposedCurrencyId);
        result.Reservation.Comment.Should().Be(reservation.Comment);
    }

    [Fact]
    public async Task MissingReservation_ThrowsNotFoundException()
    {
        var act = () => Mediator.Send(
            new GetProductReservationQuery(int.MaxValue));

        await act.Should().ThrowAsync<ReservationNotFoundException>();
    }
}
