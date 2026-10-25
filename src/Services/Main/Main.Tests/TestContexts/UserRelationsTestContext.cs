using Main.Entities.Organization;
using StorageEntity = Main.Entities.Storage.Storage;
using Main.Entities.User;
using Main.Enums;
using Main.Enums.Organization;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Organization;
using Tests.DataBuilders.Storage;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class UserRelationsTestContext(DContext context, UsersTestContext usersTestContext)
	: TestContextBase<DContext>(context), IDependentTestContext
{
	public static Type[] DependsOn { get; } = [typeof(UsersTestContext)];

	public User FirstUser { get; private set; } = null!;
	public User SecondUser { get; private set; } = null!;
	public User OtherUser { get; private set; } = null!;
	public User IndividualOnlyUser { get; private set; } = null!;
	public StorageEntity SharedStorage { get; private set; } = null!;
	public StorageEntity FirstUserStorage { get; private set; } = null!;
	public StorageEntity OtherStorage { get; private set; } = null!;
	public Organization SharedOrganization { get; private set; } = null!;
	public Organization FirstUserOrganization { get; private set; } = null!;
	public Organization OtherOrganization { get; private set; } = null!;
	public Organization HiddenOrganization { get; private set; } = null!;

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var users = usersTestContext.Users.ToArray();
		FirstUser = users[0];
		SecondUser = users[1];
		OtherUser = users[2];
		IndividualOnlyUser = await new UserBuilder(Faker).BuildAndAddToDb(DbContext);

		SharedStorage = await new StorageBuilder(Faker)
			.WithCode("shared-storage")
			.WithType(StorageType.Warehouse)
			.WithDescription("Shared warehouse")
			.WithLocation("Moscow")
			.WithOwner(FirstUser.Id)
			.WithOwner(SecondUser.Id)
			.BuildAndAddToDb(DbContext);
		FirstUserStorage = await new StorageBuilder(Faker)
			.WithCode("first-storage")
			.WithType(StorageType.Warehouse)
			.WithOwner(FirstUser.Id)
			.BuildAndAddToDb(DbContext);
		OtherStorage = await new StorageBuilder(Faker)
			.WithCode("other-storage")
			.WithType(StorageType.Warehouse)
			.WithOwner(OtherUser.Id)
			.BuildAndAddToDb(DbContext);

		SharedOrganization = await new OrganizationBuilder(Faker)
			.WithName("Shared organization")
			.WithSystemName("shared-organization")
			.WithOwnerId(FirstUser.Id)
			.WithMember(SecondUser.Id, OrganizationRole.Manager)
			.BuildAndAddToDb(DbContext);
		FirstUserOrganization = await new OrganizationBuilder(Faker)
			.WithName("First organization")
			.WithOwnerId(FirstUser.Id)
			.BuildAndAddToDb(DbContext);
		OtherOrganization = await new OrganizationBuilder(Faker)
			.WithOwnerId(OtherUser.Id)
			.BuildAndAddToDb(DbContext);
		HiddenOrganization = await new OrganizationBuilder(Faker)
			.WithOwnerId(FirstUser.Id)
			.WithIsHidden(true)
			.BuildAndAddToDb(DbContext);
	}
}
