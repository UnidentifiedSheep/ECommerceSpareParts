using Application.Common.Abstractions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Services.Events;
using Main.Modules.Identity.Abstractions;
using Main.Entities.Organization;

namespace Main.Application.DomainEventHandlers.User.UserCreated;

public class CreateIndividualOrganizationHandler(IUnitOfWork unitOfWork)
	: BatchableDomainEventHandler<UserRegistered>
{
	public override async Task Handle(
		Batch<UserRegistered> notification,
		CancellationToken cancellationToken)
	{
		foreach (var item in notification.Items)
			await unitOfWork.AddAsync(
				Organization.CreateIndividual($"{item.Name} {item.Surname}", item.UserId),
				cancellationToken);
	}
}
