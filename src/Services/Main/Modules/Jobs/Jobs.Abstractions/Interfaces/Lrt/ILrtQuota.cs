using NamedObject.Core.Interfaces;

namespace Application.Common.Interfaces.Lrt;

public interface ILrtQuota : INamedObject
{
	int MaxParallelPerWorker { get; }
}
