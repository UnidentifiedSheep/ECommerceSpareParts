using Abstractions.Interfaces.Validators;
using Abstractions.Models.Options;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Main.Application.Handlers.BaseValidators;
using Main.Entities;
using Main.Entities.User;
using Locan.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Users.CreateUser;

public class CreateUserValidation : AbstractValidator<CreateUserCommand>
{
	public CreateUserValidation(
		IPasswordManager passwordManager,
		IEmailValidator emailValidator,
		IOptions<UserEmailOptions> emailOptions,
		IOptions<UserPhoneOptions> phoneOptions)
	{
		RuleFor(x => x.UserName).SetValidator(new LoginValidator());

		RuleFor(x => x.Password).SetValidator(new PasswordValidator(passwordManager));

		RuleFor(x => x.Emails)
			.ChildRules(z => z.RuleForEach(x => x).SetValidator(new EmailDtoValidator(emailValidator)))
			.Custom((z, context) =>
			{
				var list = z.ToList();
				var primaryCount = 0;
				var setOfEmails = new HashSet<string>();
				foreach (var email in list)
				{
					setOfEmails.Add(email.Email.ToNormalizedEmail());
					if (email.IsPrimary)
						primaryCount++;
				}

				if (primaryCount > 1)
					context.AddFailure(CreateFailure(
						context.PropertyPath,
						UserEmailPrimaryCountMessage.Instance));

				if (list.Count > setOfEmails.Count)
					context.AddFailure(CreateFailure(
						context.PropertyPath,
						UserHaveDuplicateEmailMessage.Instance));

				if (list.Count < emailOptions.Value.MinEmailCount)
					context.AddFailure(CreateFailure(
						context.PropertyPath,
						new UserMinEmailCountMessage().WithCount(emailOptions.Value.MinEmailCount)));

				if (list.Count > emailOptions.Value.MaxEmailCount)
					context.AddFailure(CreateFailure(
						context.PropertyPath,
						new UserMaxEmailCountMessage().WithCount(emailOptions.Value.MaxEmailCount)));
			});

		RuleFor(x => x.Phones)
			.ChildRules(z => z.RuleForEach(x => x))
			.Custom((z, context) =>
			{
				var list = z.ToList();
				var primaryCount = 0;
				var setOfPhones = new HashSet<string>();
				foreach (var phone in list)
				{
					setOfPhones.Add(UserPhone.ToNormalizedPhone(phone.Number));
					if (phone.IsPrimary)
						primaryCount++;
				}

				if (primaryCount > 1)
					context.AddFailure(CreateFailure(
						context.PropertyPath,
						UserPhonePrimaryCountMessage.Instance));

				if (list.Count > setOfPhones.Count)
					context.AddFailure(CreateFailure(
						context.PropertyPath,
						UserHaveDuplicatePhoneMessage.Instance));

				if (list.Count < phoneOptions.Value.MinPhoneCount)
					context.AddFailure(CreateFailure(
						context.PropertyPath,
						new UserMinPhoneCountMessage().WithCount(phoneOptions.Value.MinPhoneCount)));

				if (list.Count > phoneOptions.Value.MaxPhoneCount)
					context.AddFailure(CreateFailure(
						context.PropertyPath,
						new UserMaxPhoneCountMessage().WithCount(phoneOptions.Value.MaxPhoneCount)));
			});

		RuleFor(x => x.UserInfo).SetValidator(new UserInfoValidator());
	}

	private static ValidationFailure CreateFailure(string propertyName, ILocalizableMessage message)
	{
		return new ValidationFailure(propertyName, "Validation failed")
		{
			ErrorCode = message.MessageKey,
			CustomState = message
		};
	}
}
