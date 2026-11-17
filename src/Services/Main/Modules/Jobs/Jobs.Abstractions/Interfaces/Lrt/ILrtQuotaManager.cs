using System.Diagnostics.CodeAnalysis;
using NamedObject.Core.Interfaces;

namespace Application.Common.Interfaces.Lrt;

public interface ILrtQuotaManager : INamedObject
{
	int AvailableQuota { get; }

	int MaxQuota { get; }

	ILrtQuotaLease UseQuota(Guid holderId);

	ValueTask<ILrtQuotaLease> UseQuotaAsync(Guid holderId, CancellationToken cancellationToken = default);

	bool TryUseQuota(Guid holderId, [NotNullWhen(true)] out ILrtQuotaLease? quota);
}
