using System.Diagnostics.CodeAnalysis;
using Application.Common.LRT;

namespace Application.Common.Interfaces.Lrt;

public interface ILrtQuotaManager
{
    int AvailableQuota { get; }
    int MaxQuota { get; }
    
    ILrtQuota UseQuota(Guid holderId);
    ValueTask<ILrtQuota> UseQuotaAsync(
        Guid holderId,
        CancellationToken cancellationToken = default);
    bool TryUseQuota(
        Guid holderId, 
        [NotNullWhen(true)]
        out ILrtQuota? quota);
}
