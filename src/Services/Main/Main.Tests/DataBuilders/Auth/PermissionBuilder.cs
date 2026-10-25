using Bogus;
using Enums;
using Main.Entities.Auth;
using Tests.Abstractions;

namespace Tests.DataBuilders.Auth;

public class PermissionBuilder(Faker faker) : BuilderBase<Permission>(faker)
{
	public PermissionCodes Code { get; private set; } = PermissionCodes.USERS_GET;

	public PermissionBuilder WithCode(PermissionCodes code)
	{
		Code = code;
		return this;
	}

	public override Permission Build() => new(Code);
}
