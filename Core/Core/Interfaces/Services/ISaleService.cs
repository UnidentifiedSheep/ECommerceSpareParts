using Core.Entities;
using Core.Models;

namespace Core.Interfaces.Services;

public interface ISaleService
{
    Dictionary<int, Queue<SaleContentDetail>> GetDetailsGroup(
        IEnumerable<PrevAndNewValue<StorageContent>> storageContentValues);
}