using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Users;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Entities.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Users.GetUserEmail;

public record GetUserEmailQuery(Guid UserId, string Email)
    : IQuery<GetUserEmailResult>;

public record GetUserEmailResult(UserEmailDto Email);

public class GetUserEmailHandler(
    IReadRepository<UserEmail, string> repository,
    IProjectionProvider<UserEmail, UserEmailDto> projection)
    : IQueryHandler<GetUserEmailQuery, GetUserEmailResult>
{
    public async Task<GetUserEmailResult> Handle(
        GetUserEmailQuery request,
        CancellationToken cancellationToken)
    {
        Email email = request.Email;
        var result = await repository.Query
            .Where(x => x.UserId == request.UserId && x.Email == email)
            .Project(projection)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UserEmailNotFoundException(email.Value);

        return new GetUserEmailResult(result);
    }
}
