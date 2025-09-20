namespace Core.Interfaces.Validators;

public interface IEmailValidator
{
    bool IsValidEmail(string email);
}