namespace Exceptions.Base;

public class ConcurrencyCodeMismatchException(string? clientCode, string? serverCode) :
    ConflictException("Не удалось валидировать данные. Пожалуйста, обновите страницу и попробуйте ещё раз.",
        new { ClientCode = clientCode, ServerCode = serverCode })
{
    public string? ClientCode { get; } = clientCode;
    public string? ServerCode { get; } = serverCode;
}