using System.Globalization;
using Core.Interface;
using Core.Redis;
using FluentValidation;
using MediatR;
using MonoliteUnicorn.Services.Balances;
using StackExchange.Redis;

namespace MonoliteUnicorn.EndPoints.Balances.ChangeDiscountOfUser;

public record ChangeDiscountForUserCommand(string UserId, decimal Discount) : ICommand<Unit>;

public class ChangeDiscountForUserValidation : AbstractValidator<ChangeDiscountForUserCommand>
{
    public ChangeDiscountForUserValidation()
    {
        RuleFor(command => command.UserId).NotEmpty().WithMessage("Айди пользователя не может быть пустым");
        RuleFor(command => command.Discount).GreaterThanOrEqualTo(0).WithMessage("Скидка не может быть отрицательной");
        RuleFor(command => command.Discount).LessThanOrEqualTo(100).WithMessage("Скидка не может быть больше чем 100%");
    }
}

public class ChangeDiscountForUserHandler(IBalance balance) : ICommandHandler<ChangeDiscountForUserCommand, Unit>
{
    public async  Task<Unit> Handle(ChangeDiscountForUserCommand request, CancellationToken cancellationToken)
    {
        await balance.ChangeUsersDiscount(request.UserId, request.Discount, cancellationToken);
        var redis = Redis.GetRedis();
        var key = $"userDiscount:{request.UserId}";
        await redis.StringSetAsync(new RedisKey(key), new RedisValue(request.Discount.ToString(CultureInfo.InvariantCulture)));
        return Unit.Value;
    }
}