using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IMarkupRepository
{
    Task<MarkupGroup?> GetGeneratedMarkupsAsync(bool track = true, CancellationToken cancellationToken = default);
    Task<MarkupGroup?> GetMarkupByIdAsync(int id, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<MarkupGroup>> GetMarkupGroups(int page, int viewCound, bool track = true,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<MarkupRange>> GetMarkupRanges(int markupId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<bool> MarkupExists(int markupId, CancellationToken cancellationToken = default);
}