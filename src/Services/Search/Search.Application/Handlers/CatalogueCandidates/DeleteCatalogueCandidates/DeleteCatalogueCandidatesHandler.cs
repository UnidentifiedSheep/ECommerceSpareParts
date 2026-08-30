using Application.Common.Interfaces.Cqrs;
using MediatR;
using Search.Application.Interfaces.CatalogueCandidate;

namespace Search.Application.Handlers.CatalogueCandidates.DeleteCatalogueCandidates;

public sealed record DeleteCatalogueCandidatesCommand(IReadOnlyCollection<Guid> Ids) : ICommand;

public sealed class DeleteCatalogueCandidatesHandler(ICatalogueCandidateRepository repository)
	: ICommandHandler<DeleteCatalogueCandidatesCommand>
{
	public async Task<Unit> Handle(
		DeleteCatalogueCandidatesCommand request,
		CancellationToken cancellationToken)
	{
		await repository.DeleteMany(request.Ids.Distinct(), cancellationToken);

		return Unit.Value;
	}
}
