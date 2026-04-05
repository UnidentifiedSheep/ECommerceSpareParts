using Abstractions.Interfaces.Services;
using Abstractions.Interfaces.Validators;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Main.Abstractions.Dtos.RepositoryOptionsData;
using Main.Abstractions.Exceptions.Auth;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.Auth.ChangePassword;

[AutoSave]
[Transactional]
public record ChangePasswordCommand(
    Guid UserId,
    string PreviousPassword,
    string NewPassword) : ICommand;

public class ChangePasswordHandler(
    IUserRepository userRepository,
    IPasswordManager passwordManager,
    IUserTokenRepository tokenRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUser(request, cancellationToken);
        
        if (!passwordManager.VerifyHashedPassword(user.PasswordHash, request.PreviousPassword))
            throw new WrongCredentialsException(null, request.PreviousPassword);
        
        user.PasswordHash = passwordManager.GetHashOfPassword(request.NewPassword);
        
        var tokens = await GetAllRefreshTokens(user.Id, cancellationToken);
        unitOfWork.RemoveRange(tokens);
        
        return Unit.Value;
    }

    private async Task<IReadOnlyList<UserToken>> GetAllRefreshTokens(Guid userId, CancellationToken cancellationToken)
    {
        var queryOptions = new QueryOptions<UserToken, GetUserTokensOptionsData>()
        {
            Data = new GetUserTokensOptionsData
            {
                UserId = userId,
                TokenType = TokenType.RefreshToken
            }
        }.WithTracking();
        
        return await tokenRepository.GetTokensAsync(queryOptions, cancellationToken);
    }

    private async Task<User> GetUser(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var queryOptions = new QueryOptions<User, Guid>()
            {
                Data = request.UserId
            }.WithTracking()
            .WithForUpdate();
        return await userRepository.GetUserByIdAsync(queryOptions, cancellationToken)
                   ?? throw new UserNotFoundException(request.UserId);
    }
}