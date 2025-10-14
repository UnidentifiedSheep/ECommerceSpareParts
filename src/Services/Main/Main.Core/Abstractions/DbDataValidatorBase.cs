using System.Linq.Expressions;
using Main.Core.Interfaces.Validation;
using Main.Core.Models;

namespace Main.Core.Abstractions;

public abstract class DbDataValidatorBase
{
    /// <summary>
    /// Выполняет валидацию и возвращает массив исключений, если они возникли.
    /// </summary>
    /// <param name="validationPlan">План валидации</param>
    /// <param name="throwImid">
    ///     Если <c>true</c>, исключения будут выброшены сразу же. 
    ///     Если <c>false</c>, исключения будут возвращены в массиве без выброса.
    /// </param>
    /// <param name="throwGrouped">
    ///     Если <c>true</c>, ошибки будут сгруппированы в одно исключение. 
    ///     Если <c>false</c>, будет возвращена только первая ошибка.  
    ///     <note type="important">
    ///         Параметр <paramref name="throwGrouped"/> работает только, если <paramref name="throwImid"/> установлен в <c>true</c>.
    ///     </note>
    /// </param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Массив исключений, возникших в процессе валидации.</returns>
    public abstract Task<Exception[]> Validate(IValidationPlan validationPlan, bool throwImid = true, 
        bool throwGrouped = true, CancellationToken cancellationToken = default);
}