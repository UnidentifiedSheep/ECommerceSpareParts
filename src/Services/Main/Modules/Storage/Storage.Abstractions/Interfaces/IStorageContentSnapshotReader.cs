using Main.Modules.Storage.Abstractions.Events;

namespace Main.Modules.Storage.Abstractions.Interfaces;

public interface IStorageContentSnapshotReader
{
	Task<IReadOnlyList<StorageContentSnapshot>> GetAfterAsync(
		int afterStorageContentId,
		int take,
		CancellationToken cancellationToken = default);
}
