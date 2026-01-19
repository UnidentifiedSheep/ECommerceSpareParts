using Bogus;
using Exceptions.Exceptions.Cart;
using Main.Application.Handlers.Articles.CreateArticles;
using Main.Application.Handlers.Cart.AddToCart;
using Main.Application.Handlers.Cart.ChangeCartItemCount;
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

namespace Tests.HandlersTests.Cart;

[Collection("Combined collection")]
public class ChangeCartItemCountTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new(MockData.MockData.Locale);
    private readonly IMediator _mediator;
    private Guid _userId;
    private int _articleId;

    public ChangeCartItemCountTests(CombinedContainerFixture fixture)
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

        // Add to Cart
        var addToCartCommand = new AddToCartCommand(_userId, _articleId, 5);
        await _mediator.Send(addToCartCommand);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task ChangeCartItemCount_ValidData_Succeeds()
    {
        var newCount = _faker.Random.Int(1, 100);
        var command = new ChangeCartItemCountCommand(_userId, _articleId, newCount);

        await _mediator.Send(command);

        var cartItem = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == _userId && x.ArticleId == _articleId);
        Assert.NotNull(cartItem);
        Assert.Equal(newCount, cartItem.Count);
    }

    [Fact]
    public async Task ChangeCartItemCount_ItemNotFound_ThrowsCartItemNotFoundException()
    {
        var command = new ChangeCartItemCountCommand(_userId, 999999, 10);

        await Assert.ThrowsAsync<CartItemNotFoundException>(() => _mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ChangeCartItemCount_InvalidCount_ThrowsValidationException(int newCount)
    {
        var command = new ChangeCartItemCountCommand(_userId, _articleId, newCount);

        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(command));
    }
}
