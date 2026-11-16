using Main.Entities.Organization;
using Main.Entities.Organization.Enums.Organization;
using Main.Entities.Organization.Organization;
using Main.Entities.User;
using Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Organization;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class UserRelationsTestContext(UnifiedDbContext context, UsersTestContext usersTestContext)
	: TestContextBase<UnifiedDbContext>(context), IDependentTestContext
{

	public User FirstUser { get; private set; } = null!;
	public User SecondUser { get; private set; } = null!;
	public User OtherUser { get; private set; } = null!;
	public User IndividualOnlyUser { get; private set; } = null!;
	public Organization SharedOrganization { get; private set; } = null!;
	public Organization FirstUserOrganization { get; private set; } = null!;
	public Organization OtherOrganization { get; private set; } = null!;
	public Organization HiddenOrganization { get; private set; } = null!;
	public static Type[] DependsOn { get; } = [typeof(UsersTestContext)];

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var users = usersTestContext.Users.ToArray();
		FirstUser = users[0];
		SecondUser = users[1];
		OtherUser = users[2];
		IndividualOnlyUser = await new UserBuilder(Faker).BuildAndAddToDb(DbContext);

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
