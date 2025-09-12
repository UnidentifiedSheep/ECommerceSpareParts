using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Currencies;

public class CannotDeleteUsdException : BadRequestException
{
    public CannotDeleteUsdException() : base("Нельзя удалить доллар.")
    {
    }
}