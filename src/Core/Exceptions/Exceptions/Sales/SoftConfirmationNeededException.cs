using Core.Attributes;
using Core.Interfaces.Exceptions;
using Exceptions.Base;

namespace Exceptions.Exceptions.Sales;

public class SoftConfirmationNeededException : PreconditionRequiredException
{
    [ExampleExceptionValues(false, typeof(SoftConfirmationExample))]
    public SoftConfirmationNeededException(string? confirmationCode, Dictionary<string, int> reserved) :
        base("Продажа требует подтверждения из-за конфликта с текущими резервациями.",
            new { ConfirmationCode = confirmationCode ?? "", Reserved = reserved })
    {
    }
}

public class SoftConfirmationExample : IExceptionExample
{
    public string ConfirmationCode { get; set; } = "Super_Code";
    public Dictionary<string,int> Reserved { get; set; } = new() { ["item1"]=3, ["item2"]=7 };
}