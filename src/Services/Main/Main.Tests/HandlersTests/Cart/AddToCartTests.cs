using Bogus;
using Main.Abstractions.Constants;
using Main.Application.Handlers.Articles.CreateArticles;
using Main.Application.Handlers.Cart.AddToCart;
using Main.Application.Handlers.Producers.CreateProducer;
using Main.Application.Handlers.Users.CreateUser;
using Main.Abstractions.Dtos.Emails;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Cart;

[Collection("Combined collection")]
public class AddToCartTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new(MockData.MockData.Locale);
    private readonly IMediator _mediator;
    private Guid _userId;
    private int _articleId;

    public AddToCartTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }

    public async Task InitializeAsync()
    {
        // Create User
        var email = new EmailDto
        {
            Email = _faker.Person.Email,
            IsConfirmed = false,
            IsPrimary = true,
            Type = EmailType.Personal
        };
        var userInfo = MockData.MockData.CreateUserInfoDto();
        var createUserCommand = new CreateUserCommand(_faker.Person.UserName, _faker.Lorem.Letter(10),
            userInfo, [email], [], []);
        var userResult = await _mediator.Send(createUserCommand);
        _userId = userResult.UserId;

        // Create Producer
        var newProducerModel = MockData.MockData.CreateNewProducerDto(1)[0];
        var createProducerCommand = new CreateProducerCommand(newProducerModel);
        var producerResult = await _mediator.Send(createProducerCommand);

        // Create Article
        var articleList = MockData.MockData.CreateNewArticleDto(1);
        articleList[0].ProducerId = producerResult.ProducerId;
        var createArticleCommand = new CreateArticlesCommand(articleList);
        var articleResult = await _mediator.Send(createArticleCommand);
        _articleId = articleResult.CreatedIds[0];
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task AddToCart_ValidData_Succeeds()
    {
        var count = _faker.Random.Int(1, 100);
        var command = new AddToCartCommand(_userId, _articleId, count);

        await _mediator.Send(command);

        var cartItem = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == _userId && x.ArticleId == _articleId);
        Assert.NotNull(cartItem);
        Assert.Equal(count, cartItem.Count);
    }

    [Fact]
    public async Task AddToCart_SameItem_ThrowsSameItemInCartException()
    {
        var command = new AddToCartCommand(_userId, _articleId, 5);
        await _mediator.Send(command);

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(command));
        Assert.Equal(ApplicationErrors.CartItemAlreadyExist, exception.Failures[0].ErrorName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddToCart_InvalidCount_ThrowsValidationException(int count)
    {
        var command = new AddToCartCommand(_userId, _articleId, count);

        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(command));
    }

    [Fact]
    public async Task AddToCart_UserNotFound_ThrowsUserNotFoundException()
    {
        var command = new AddToCartCommand(Guid.NewGuid(), _articleId, 1);

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(command));
        Assert.Equal(ApplicationErrors.UsersNotFound, exception.Failures[0].ErrorName);
    }
}
