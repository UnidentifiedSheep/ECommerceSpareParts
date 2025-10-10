using Main.Core.Entities;
using Main.Core.Models;

namespace Main.Core.Interfaces.Services;

public interface ISaleService
{
    Dictionary<int, Queue<SaleContentDetail>> GetDetailsGroup(
        IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues);
}