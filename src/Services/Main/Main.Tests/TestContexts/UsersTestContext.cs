using Enums;
using Main.Entities.Auth;
using Main.Entities.User;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Auth;
using Tests.DataBuilders.User;
using Tests.Extensions;

namespace Tests.TestContexts;

public class UsersTestContext(DContext context) : TestContextBase<DContext>(context)
{
	public IReadOnlyCollection<User> Users { get; private set; } = null!;

	public User User { get; private set; } = null!;
	public User OtherUser { get; private set; } = null!;
	public User ThirdUser { get; private set; } = null!;
	public Main.Entities.Auth.Role ReaderRole { get; private set; } = null!;
	public Main.Entities.Auth.Role EditorRole { get; private set; } = null!;
	public string ReadPermission => Permission.ToNormalizedPermission(PermissionCodes.USERS_GET);
	public string InfoPermission => Permission.ToNormalizedPermission(PermissionCodes.USERS_INFO_GET);
	public string CreatePermission => Permission.ToNormalizedPermission(PermissionCodes.USERS_CREATE);

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		Users = await new MemberUserBuilder(Faker).BuildManyAndAddToDb(DbContext, 3);
		User = Users.First();
		OtherUser = Users.ElementAt(1);
		ThirdUser = Users.Last();
		await new PermissionBuilder(Faker).WithCode(PermissionCodes.USERS_GET).BuildAndAddToDb(DbContext);
		await new PermissionBuilder(Faker).WithCode(PermissionCodes.USERS_INFO_GET).BuildAndAddToDb(DbContext);
		await new PermissionBuilder(Faker).WithCode(PermissionCodes.USERS_CREATE).BuildAndAddToDb(DbContext);

		ReaderRole = new RoleBuilder(Faker).WithName("batch-reader").Build();
		ReaderRole.AddPermission(InfoPermission);
		DbContext.Add(ReaderRole);
		EditorRole = await new RoleBuilder(Faker).WithName("batch-editor").BuildAndAddToDb(DbContext);
		User.AddRole(ReaderRole.Name);
		OtherUser.AddRole(EditorRole.Name);
		DbContext.AddRange(
			UserPermission.Create(User.Id, ReadPermission),
			UserPermission.Create(User.Id, InfoPermission),
			UserPermission.Create(OtherUser.Id, CreatePermission));
		await DbContext.SaveChangesAsync(cancellationToken);
	}
}
