using Application.Interfaces;
using Core.Attributes;
using Core.Entities;
using Exceptions.Base;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Contexts;
using Persistence.Entities;

namespace Application.Handlers.Auth.Register;

[Transactional]
public record RegisterCommand(string Email, string UserName, string Password, string Name, string Surname)
    : ICommand<Unit>;

internal class RegisterHandler(UserManager<UserModel> manager) : ICommandHandler<RegisterCommand, Unit>
{
    public async Task<Unit> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new UserModel
        {
            Email = request.Email,
            Name = request.Name,
            Surname = request.Surname,
            UserName = request.UserName,
        };
        //TODO: Change to custom Auth. RvsSecureBack repo.
        var result = await manager.CreateAsync(user, request.Password);
        string errors = string.Join('\n', result.Errors.Select(x => x.Description));
        if (!result.Succeeded) throw new BadRequestException("Registration Failed", errors);
        await manager.AddToRoleAsync(user, "Member");
        return Unit.Value;
    }
}