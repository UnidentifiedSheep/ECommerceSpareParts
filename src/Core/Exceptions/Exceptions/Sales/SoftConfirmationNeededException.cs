using Core.Attributes;
using Exceptions.Base;

namespace Exceptions.Exceptions.Sales;

public class SoftConfirmationNeededException : PreconditionRequiredException
{
    public SoftConfirmationNeededException(string? confirmationCode, Dictionary<string, int> reserved) :
        base("Продажа требует подтверждения из-за конфликта с текущими резервациями.",
            new { ConfirmationCode = confirmationCode ?? "", Reserved = reserved })
    {
    }
}