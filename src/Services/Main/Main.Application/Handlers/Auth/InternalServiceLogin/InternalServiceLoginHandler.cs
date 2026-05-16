using Abstractions.Interfaces.Validators;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Exceptions.Auth;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;

namespace Main.Application.Handlers.Auth.InternalServiceLogin;

public record InternalServiceLoginCommand(string Service, string ServiceSecret) : ICommand<InternalServiceLoginResult>;

public record InternalServiceLoginResult(string Token);

public class InternalServiceLoginHandler(
    IPasswordManager passwordManager,
    IUserRepository userRepository,
    IJwtGenerator tokenGenerator,
    IUserService userService) : ICommandHandler<InternalServiceLoginCommand, InternalServiceLoginResult>
{
    public async Task<InternalServiceLoginResult> Handle(
        InternalServiceLoginCommand request,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<User>.New()
            .Include(x => x.UserInfo)
            .Where(x => x.UserName.NormalizedValue == UserName.ToNormalized(request.Service))
            .Build();

        var user = await userRepository.FirstOrDefaultAsync(criteria, cancellationToken)
                   ?? throw new WrongCredentialsException(request.Service, request.ServiceSecret);
        if (!passwordManager.VerifyHashedPassword(user.PasswordHash, request.ServiceSecret))
            throw new WrongCredentialsException(request.Service, request.ServiceSecret);

        var (roles, permissions) =
            await userService.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken)
            ?? throw new UserNotFoundException(user.Id);

        var userDto = UserProjections.UserProjection.AsFunc()(user);
        var token = tokenGenerator.CreateToken(userDto, request.Service, roles, permissions);
        return new InternalServiceLoginResult(token);
    }
}