using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Main.Application.Extensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions;
using Main.Entities.User;
using MediatR;

namespace Main.Application.Handlers.Users.MakeEmailPrimary;

[Transactional]
[AutoSave]
public record MakeEmailPrimaryCommand(Guid UserId, string Email) : ICommand;

public class MakeEmailPrimaryHandler(IUserRepository repository) : ICommandHandler<MakeEmailPrimaryCommand>
{
	public async Task<Unit> Handle(MakeEmailPrimaryCommand request, CancellationToken cancellationToken)
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

		user.MakeEmailPrimary(request.Email);

		return Unit.Value;
	}
}
