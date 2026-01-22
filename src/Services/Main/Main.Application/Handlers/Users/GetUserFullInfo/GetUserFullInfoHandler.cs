using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Users;
using Main.Abstractions.Dtos.Emails;
using Main.Abstractions.Dtos.Users;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Users.GetUserFullInfo;

public record GetUserFullInfoQuery(Guid UserId) : IQuery<GetUserFullInfoResult>;
public record GetUserFullInfoResult(UserInfoDto? UserInfo, List<FullEmailDto> Emails);

public class GetUserFullInfoHandler(IUserRepository userRepository, IUserEmailRepository userEmailRepository) 
    : IQueryHandler<GetUserFullInfoQuery, GetUserFullInfoResult>
{
    public async Task<GetUserFullInfoResult> Handle(GetUserFullInfoQuery request, CancellationToken cancellationToken)
    {
        var userInfo = await userRepository.GetUserInfo(request.UserId, false, cancellationToken);
        var emails = await userEmailRepository
            .GetUserEmailsAsync(request.UserId, null, null, false, cancellationToken);
        
        return new GetUserFullInfoResult(userInfo.Adapt<UserInfoDto?>(), emails.Adapt<List<FullEmailDto>>());
    }
}