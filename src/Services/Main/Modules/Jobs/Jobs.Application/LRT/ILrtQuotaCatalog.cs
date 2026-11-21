using Application.Common.Interfaces.Lrt;

namespace Main.Modules.Jobs.Application.LRT;

public interface ILrtQuotaCatalog
{
	IReadOnlyDictionary<string, ILrtQuotaManager> ManagersByLrt { get; }

	void Validate();
}
