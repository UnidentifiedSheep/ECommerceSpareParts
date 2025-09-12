using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence.Contexts;
using Persistence.Entities;

namespace Application.Handlers.Auth
{
    public record RegisterCommand(string Email, string UserName, string Password, string Name, string Surname) : ICommand<Unit>;
    internal class RegisterHandler(IdentityContext context, UserManager<UserModel> manager) : ICommandHandler<RegisterCommand, Unit>
    {
        public async Task<Unit> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            /*await using var dbTransaction = await context.Database.BeginTransactionAsync(cancellationToken); 
            var user = new UserModel
            {
                Email = request.Email,
                Name = request.Name,
                Surname = request.Surname,
                UserName = request.UserName,
            };
            var result = await manager.CreateAsync(user, request.Password);
            string errors = string.Join('\n', result.Errors.Select(x => x.Description));
            if (!result.Succeeded) throw new BadRequestException("Registration Failed", errors);
            await manager.AddToRoleAsync(user, "Member");
            var emailConfirmationToken = await manager.GenerateEmailConfirmationTokenAsync(user);
            var asHtml = $"<div><p>VERY IMP LINK </p><a href=\"{Global.BaseUrl}/auth/confirm/mail?confirmationToken={emailConfirmationToken}&userId={user.Id}\">PRESS ME TO CONFIRM</a></div>";
            await mail.SendMailAsync(user.Email, asHtml, "Подтверждение почты", cancellationToken: cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);
            return Unit.Value;*/
        }
    }
}
