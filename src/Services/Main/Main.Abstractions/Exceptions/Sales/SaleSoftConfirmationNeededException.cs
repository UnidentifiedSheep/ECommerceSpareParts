using Abstractions.Interfaces.Exceptions;
using Exceptions.Base;

namespace Main.Abstractions.Exceptions.Sales;

public class SaleSoftConfirmationNeededException : PreconditionRequiredException, ILocalizableException
{
    public SaleSoftConfirmationNeededException(string confirmationCode, Dictionary<string, int> reserved) :
        base(null, new { ConfirmationCode = confirmationCode, Reserved = reserved })
    {
        Arguments = [confirmationCode];
    }

    public string MessageKey => "soft.confirmation.needed.for.sale.reservation.reason";
    public object[]? Arguments { get; }
}