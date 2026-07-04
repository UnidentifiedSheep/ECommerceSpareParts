using FluentAssertions;
using Main.Application.Handlers.Auth.UpsertRole;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.Auth;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Roles;

public class UpsertRoleTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    public async Task UpsertRole_InvalidName_ThrowsValidation(string invalidName)
    {
        var command = new UpsertRoleCommand(invalidName, null);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task UpsertRole_ValidData_CreatesRole()
    {
        var r = new RoleBuilder(Faker).Build();
        var command = new UpsertRoleCommand(r.Name, r.Description);

        await Mediator.Send(command);

        var roleInDb = await Context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == r.Name);

        Assert.NotNull(roleInDb);

        Assert.Equal(r.Name, roleInDb.Name);
        Assert.Equal(r.Description, roleInDb.Description);
    }

    [Fact]
    public async Task UpsertRole_ValidDescription_UpdatesRole()
    {
        var role = await new RoleBuilder(Faker).BuildAndAddToDb(Context);
        var command = new UpsertRoleCommand(role.Name, "New description");

        var act = () => Mediator.Send(command);
        await act.Should().NotThrowAsync();

        var roleInDb = await Context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == role.Name);

        roleInDb.Should().NotBeNull();
        roleInDb.Description.Should().Be(command.Description);
        roleInDb.Name.Should().Be(role.Name);
    }
}