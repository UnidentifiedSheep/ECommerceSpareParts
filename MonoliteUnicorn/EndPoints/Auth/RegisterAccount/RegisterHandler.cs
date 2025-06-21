using System.Web;
using Core.Exceptions;
using Core.Interface;
using Core.Mail;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MonoliteUnicorn.PostGres.Identity;

namespace MonoliteUnicorn.EndPoints.Auth.RegisterAccount
{
    public record RegisterCommand(string Email, string UserName, string Password, string Name, string Surname) : ICommand<Unit>;
    internal class RegisterHandler(IdentityContext context, UserManager<UserModel> manager, IMail mail) : ICommandHandler<RegisterCommand, Unit>
    {
        public async Task<Unit> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            await using var dbTransaction = await context.Database.BeginTransactionAsync(cancellationToken); 
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
            return Unit.Value;
        }
    }
}
