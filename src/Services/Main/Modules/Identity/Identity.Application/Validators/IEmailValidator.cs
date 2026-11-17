namespace Main.Modules.Identity.Application.Validators;

public interface IEmailValidator
{
	bool IsValidEmail(string email);
}
