using Main.Application.Handlers.Auth.CreateRole;
using Main.Entities.Exceptions.Auth;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.Auth;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Roles;

public class CreateRoleTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    public async Task CreateRole_InvalidName_ThrowsValidation(string invalidName)
    {
        var command = new CreateRoleCommand(invalidName, null);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task CreateRole_DuplicateName_ThrowsRoleAlreadyExistsException()
    {
        var role = await new RoleBuilder(Faker)
            .BuildAndAddToDb(Context);

        var dup = new CreateRoleCommand(role.Name, "another");
        await Assert.ThrowsAsync<RoleAlreadyExistsException>(async () => await Mediator.Send(dup));
    }

    [Fact]
    public async Task CreateRole_ValidData_Succeeds()
    {
        var r = new RoleBuilder(Faker).Build();
        var command = new CreateRoleCommand(r.Name, r.Description);

        await Mediator.Send(command);

        var roleInDb = await Context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == r.Name);

        Assert.NotNull(roleInDb);

        Assert.Equal(r.Name, roleInDb.Name);
        Assert.Equal(r.Description, roleInDb.Description);
    }
}