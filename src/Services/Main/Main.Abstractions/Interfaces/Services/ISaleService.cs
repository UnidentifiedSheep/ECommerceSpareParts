using Main.Abstractions.Models;
using Main.Entities;

namespace Main.Abstractions.Interfaces.Services;

public interface ISaleService
{
    Dictionary<int, Queue<SaleContentDetail>> GetDetailsGroup(
        IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues);
}