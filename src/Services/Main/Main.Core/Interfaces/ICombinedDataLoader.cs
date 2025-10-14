using Main.Core.Models;

namespace Main.Core.Interfaces;

public interface ICombinedDataLoader
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rules">Правила для проверки</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<(ExistenceCheck rule, object[] foundValues)>> GetExistenceChecks(
        IEnumerable<ExistenceCheck> rules, CancellationToken cancellationToken = default);
}