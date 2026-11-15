using System.Diagnostics.CodeAnalysis;

namespace Application.Common.Interfaces.Lrt;

public interface ILrtQuotaManager
{
	int AvailableQuota { get; }

	int MaxQuota { get; }

	ILrtQuotaLease UseQuota(Guid holderId);

	ValueTask<ILrtQuotaLease> UseQuotaAsync(Guid holderId, CancellationToken cancellationToken = default);
	bool TryUseQuota(Guid holderId, [NotNullWhen(true)] out ILrtQuotaLease? quota);
}
