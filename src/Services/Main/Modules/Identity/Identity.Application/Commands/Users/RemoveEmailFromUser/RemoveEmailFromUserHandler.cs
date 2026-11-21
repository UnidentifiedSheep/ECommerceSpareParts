using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Main.Entities.Exceptions;
using Main.Entities.User;
using Main.Modules.Identity.Application.Extensions;
using Main.Modules.Identity.Application.Interfaces.Persistence;
using Main.Modules.Identity.Application.Models.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Modules.Identity.Application.Commands.Users.RemoveEmailFromUser;

[Transactional]
[AutoSave]
public record RemoveEmailFromUserCommand(Guid UserId, string Email) : ICommand;

public class RemoveEmailFromUserHandler(IOptions<UserEmailOptions> options, IUserRepository repository)
	: ICommandHandler<RemoveEmailFromUserCommand>
{
	public async Task<Unit> Handle(RemoveEmailFromUserCommand request, CancellationToken cancellationToken)
	{
		var criteria = Criteria<User>
			.New()
			.Where(x => x.Id == request.UserId)
			.WhereDoesNotHaveRole(Role.System)
			.Include(x => x.Emails)
			.Track()
			.Build();

		var user = await repository.FirstOrDefaultAsync(criteria, cancellationToken) ??
			throw new UserNotFoundException(request.UserId);

		user.RemoveEmail(request.Email, options.Value.MinEmailCount);

		return Unit.Value;
	}
}
