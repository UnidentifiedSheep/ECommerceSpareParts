using Exceptions;
using FluentAssertions;
using Main.Application.Dtos.Product.Reservation;
using Main.Application.Handlers.ProductReservations.CreateProductReservation;
using Main.Entities.Currency;
using Main.Entities.Product;
using Main.Entities.User;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;

namespace Tests.HandlersTests.ProductReservations;

public class CreateProductReservationTests : IntegrationTest
{
    public CreateProductReservationTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
        RegisterBasicContext<UsersTestContext>();
        RegisterBasicContext<CurrencyTestContext>();
    }

    private ProductTestContext ProductContext => GetContext<ProductTestContext>();
    private UsersTestContext UsersContext => GetContext<UsersTestContext>();
    private CurrencyTestContext CurrencyContext => GetContext<CurrencyTestContext>();

    [Fact]
    public async Task WithValidData_CreatesReservation()
    {
        var dto = GetValidDto();
        var command = new CreateProductReservationCommand(dto);

        var result = await Mediator.Send(command);

        result.Reservation.Id.Should().BeGreaterThan(0);
        result.Reservation.User.User!.Id.Should().Be(dto.UserId);
        result.Reservation.ReservedCount.Should().Be(dto.ReservedCount);
        result.Reservation.CurrentCount.Should().Be(dto.CurrentCount);
        result.Reservation.ProposedPrice.Should().Be(dto.ProposedPrice);
        result.Reservation.ProposedCurrencyId.Should().Be(dto.GivenCurrencyId);
        result.Reservation.Comment.Should().Be(dto.Comment);

        var db = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == result.Reservation.Id);

        db.Comment.Should().Be(dto.Comment);
        db.ProductId.Should().Be(dto.ProductId);
        db.UserId.Should().Be(dto.UserId);
        db.ReservedCount.Should().Be(dto.ReservedCount);
        db.CurrentCount.Should().Be(dto.CurrentCount);
        db.ProposedPrice.Should().Be(dto.ProposedPrice);
        db.ProposedCurrencyId.Should().Be(dto.GivenCurrencyId);
        db.Status.Should().Be(StorageContentReservationStatus.Locked);
    }

    [Fact]
    public async Task WithReservedCountLessThanCurrentCount_ThrowsValidationException()
    {
        var dto = GetValidDto() with
        {
            ReservedCount = 1,
            CurrentCount = 2
        };
        var command = new CreateProductReservationCommand(dto);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithCurrentCountEqualReservedCount_CreatesDoneReservation()
    {
        var dto = GetValidDto() with
        {
            ReservedCount = 3,
            CurrentCount = 3
        };
        var command = new CreateProductReservationCommand(dto);

        var result = await Mediator.Send(command);

        result.Reservation.Status.Should().Be(StorageContentReservationStatus.Done);

        var db = await Context.StorageContentReservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == result.Reservation.Id);
        db.Status.Should().Be(StorageContentReservationStatus.Done);
    }

    [Fact]
    public async Task WithoutProposedPriceAndCurrency_CreatesReservation()
    {
        var dto = GetValidDto() with
        {
            ProposedPrice = null,
            GivenCurrencyId = null
        };
        var command = new CreateProductReservationCommand(dto);

        var result = await Mediator.Send(command);

        result.Reservation.ProposedPrice.Should().BeNull();
        result.Reservation.ProposedCurrencyId.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WithInvalidReservedCount_ThrowsValidationException(int reservedCount)
    {
        var dto = GetValidDto() with
        {
            ReservedCount = reservedCount,
            CurrentCount = 1
        };
        var command = new CreateProductReservationCommand(dto);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Theory]
    [InlineData(-1)]
    public async Task WithInvalidCurrentCount_ThrowsValidationException(int currentCount)
    {
        var dto = GetValidDto() with
        {
            ReservedCount = 3,
            CurrentCount = currentCount
        };
        var command = new CreateProductReservationCommand(dto);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WithInvalidProposedPrice_ThrowsValidationException(decimal proposedPrice)
    {
        var dto = GetValidDto() with
        {
            ProposedPrice = proposedPrice,
            GivenCurrencyId = CurrencyContext.Currencies.First().Id
        };
        var command = new CreateProductReservationCommand(dto);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithPriceWithoutCurrency_ThrowsInvalidInputException()
    {
        var dto = GetValidDto() with
        {
            ProposedPrice = 100m,
            GivenCurrencyId = null
        };
        var command = new CreateProductReservationCommand(dto);

        await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithCurrencyWithoutPrice_ThrowsInvalidInputException()
    {
        var dto = GetValidDto() with
        {
            ProposedPrice = null,
            GivenCurrencyId = CurrencyContext.Currencies.First().Id
        };
        var command = new CreateProductReservationCommand(dto);

        await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithMissingProduct_ThrowsDbValidationException()
    {
        var dto = GetValidDto() with
        {
            ProductId = 999999
        };
        var command = new CreateProductReservationCommand(dto);

        await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithMissingUser_ThrowsDbValidationException()
    {
        var dto = GetValidDto() with
        {
            UserId = Guid.NewGuid()
        };
        var command = new CreateProductReservationCommand(dto);

        await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task WithMissingCurrency_ThrowsDbValidationException()
    {
        var dto = GetValidDto() with
        {
            ProposedPrice = 100m,
            GivenCurrencyId = 999999
        };
        var command = new CreateProductReservationCommand(dto);

        await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command));
    }

    private NewProductReservationDto GetValidDto()
    {
        var reservedCount = Faker.Random.Int(10, 1000);
        var hasProposedPrice = Faker.Random.Bool();
        return new NewProductReservationDto
        {
            ProductId = Faker.PickRandom<Product>(ProductContext.Products).Id,
            Comment = Faker.Lorem.Sentence(),
            GivenCurrencyId =
                hasProposedPrice ? Faker.PickRandom<Currency>(CurrencyContext.Currencies).Id : null,
            ReservedCount = reservedCount,
            CurrentCount = Faker.Random.Int(1, reservedCount - 1),
            ProposedPrice = hasProposedPrice ? Math.Round(Faker.Random.Decimal(1, 1000), 2) : null,
            UserId = Faker.PickRandom<User>(UsersContext.Users).Id
        };
    }
}