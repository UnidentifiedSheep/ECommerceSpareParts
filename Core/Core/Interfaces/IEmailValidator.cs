namespace Core.Interfaces;

public interface IEmailValidator
{
    bool IsValidEmail(string email);
}