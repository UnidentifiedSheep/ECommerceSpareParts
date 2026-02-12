namespace Abstractions.Interfaces.Validators;

public interface IEmailValidator
{
    bool IsValidEmail(string email);
}