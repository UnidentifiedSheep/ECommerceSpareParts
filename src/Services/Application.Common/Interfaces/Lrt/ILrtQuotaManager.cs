using System.Diagnostics.CodeAnalysis;
using Application.Common.LRT;

namespace Application.Common.Interfaces.Lrt;

public interface ILrtQuotaManager
{
    int AvailableQuota { get; }
    int MaxQuota { get; }
    
    ILrtQuota UseQuota(Guid holderId);
    bool TryUseQuota(
        Guid holderId, 
        [NotNullWhen(true)]
        out ILrtQuota? quota);
}