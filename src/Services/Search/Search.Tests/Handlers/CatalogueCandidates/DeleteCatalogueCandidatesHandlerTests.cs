using FluentAssertions;
using Moq;
using Search.Application.Handlers.CatalogueCandidates.DeleteCatalogueCandidates;
using Search.Application.Interfaces.CatalogueCandidate;

namespace Search.Tests.Handlers.CatalogueCandidates;

public class DeleteCatalogueCandidatesHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteDistinctCandidateIds()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        List<Guid> deletedIds = [];
        var repository = new Mock<ICatalogueCandidateRepository>();
        repository
            .Setup(x => x.DeleteMany(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Guid>, CancellationToken>(
                (ids, _) => deletedIds.AddRange(ids))
            .Returns(Task.CompletedTask);
        var handler = new DeleteCatalogueCandidatesHandler(repository.Object);

        await handler.Handle(
            new DeleteCatalogueCandidatesCommand([firstId, secondId, firstId]),
            CancellationToken.None);

        deletedIds.Should().Equal(firstId, secondId);
    }
}
