using Main.Modules.Storage.Abstractions.Events;

namespace Main.Application.Interfaces.Services;

public interface IStorageContentSnapshotReader
{
	Task<IReadOnlyList<StorageContentSnapshot>> GetAfterAsync(
		int afterStorageContentId,
		int take,
		CancellationToken cancellationToken = default);
}
