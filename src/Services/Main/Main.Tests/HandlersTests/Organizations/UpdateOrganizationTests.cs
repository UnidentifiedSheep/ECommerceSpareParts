using Abstractions.Models;
using FluentAssertions;
using Main.Application.Dtos.Organizations;
using Main.Application.Handlers.Organizations.UpdateOrganization;
using Main.Entities.Exceptions;
using Main.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.Organization;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Organizations;

public sealed class UpdateOrganizationTests : IntegrationTest
{
    public UpdateOrganizationTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<UsersTestContext>();
    }

    [Fact]
    public async Task UpdateOrganization_Name_UpdatesOrganization()
    {
        var organization = await CreateOrganization();
        var command = new UpdateOrganizationCommand(
            organization.Id,
            new PatchOrganizationDto
            {
                Name = PatchField<string>.From("  Updated organization  ")
            });

        var result = await Mediator.Send(command);

        result.OrganizationId.Should().Be(organization.Id);
        var updatedOrganization = await Context.Organizations
            .AsNoTracking()
            .SingleAsync(x => x.Id == organization.Id);
        updatedOrganization.Name.Should().Be("Updated organization");
    }

    [Fact]
    public async Task UpdateOrganization_NoFieldsSet_PreservesOrganization()
    {
        var organization = await CreateOrganization();

        var result = await Mediator.Send(
            new UpdateOrganizationCommand(
                organization.Id,
                new PatchOrganizationDto()));

        result.OrganizationId.Should().Be(organization.Id);
        var dbOrganization = await Context.Organizations
            .AsNoTracking()
            .SingleAsync(x => x.Id == organization.Id);
        dbOrganization.Name.Should().Be(organization.Name);
        dbOrganization.SystemName.Should().Be(organization.SystemName);
    }

    [Fact]
    public async Task UpdateOrganization_MissingOrganization_ThrowsNotFoundException()
    {
        var organizationId = Guid.NewGuid();
        var command = new UpdateOrganizationCommand(
            organizationId,
            new PatchOrganizationDto
            {
                Name = PatchField<string>.From("Updated organization")
            });

        var act = () => Mediator.Send(command);

        await act.Should().ThrowAsync<OrganizationNotFoundException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ab")]
    public async Task UpdateOrganization_InvalidName_ThrowsValidationException(
        string? name)
    {
        var organization = await CreateOrganization();
        var command = new UpdateOrganizationCommand(
            organization.Id,
            new PatchOrganizationDto
            {
                Name = PatchField<string>.From(name!)
            });

        var act = () => Mediator.Send(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private async Task<Organization> CreateOrganization()
    {
        var owner = GetContext<UsersTestContext>().Users.First();
        var organization = await new OrganizationBuilder(Faker)
            .WithOwnerId(owner.Id)
            .WithName("Original organization")
            .BuildAndAddToDb(Context);
        Context.ChangeTracker.Clear();
        return organization;
    }
}
