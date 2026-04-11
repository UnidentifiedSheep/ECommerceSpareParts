using Main.Abstractions.Models;
using Main.Entities;
using Main.Entities.Sale;
using Main.Entities.Storage;

namespace Main.Abstractions.Interfaces.Services;

public interface ISaleService
{
    Dictionary<int, Queue<SaleContentDetail>> GetDetailsGroup(
        IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues);
}