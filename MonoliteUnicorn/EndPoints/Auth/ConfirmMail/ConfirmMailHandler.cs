using Core.Interface;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MonoliteUnicorn.Exceptions;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Identity;

namespace MonoliteUnicorn.EndPoints.Auth.ConfirmMail;

public record ConfirmMailQuery(string UserId, string ConfirmationToken) : IQuery<Unit>;
public class ConfirmMailHandler(UserManager<UserModel> manager) : IQueryHandler<ConfirmMailQuery, Unit>
{
    public async Task<Unit> Handle(ConfirmMailQuery request, CancellationToken cancellationToken)
    {
        var user = await manager.FindByIdAsync(request.UserId);
        if (user == null) throw new UserNotFoundException();
        var confirmed = await manager.ConfirmEmailAsync(user, request.ConfirmationToken);
        if (!confirmed.Succeeded) throw new InvalidTokenException($"{request.ConfirmationToken} не является валидным.");
        return Unit.Value;
    }
}