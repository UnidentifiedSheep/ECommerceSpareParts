using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions;

public class ConcurrencyCodeMismatchException(string? clientCode, string? serverCode) : ConflictException("Не удалось валидировать данные. Пожалуйста, обновите страницу и попробуйте ещё раз.")
{
    public string? ClientCode { get; } = clientCode;
    public string? ServerCode { get; } = serverCode;
}