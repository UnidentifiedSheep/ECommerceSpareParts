using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Currencies;

public class CannotDeleteUsdException : BadRequestException
{
    public CannotDeleteUsdException() : base("Нельзя удалить доллар.")
    {
    }
}