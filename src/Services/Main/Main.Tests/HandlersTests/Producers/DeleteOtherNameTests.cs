using FluentAssertions;
using Main.Application.Handlers.Producers.DeleteOtherName;
using Main.Entities.Exceptions.Producers;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders;
using Tests.TestContexts.Base;

namespace Tests.HandlersTests.Producers;

public class DeleteOtherNameTests: TestBase
{
    public DeleteOtherNameTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }
    
    private ProducerOtherName _otherName = null!;
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _otherName = await new ProducerOtherNameBuilder(Faker)
            .WithProducerId(GetContext<ProducerTestContext>().Producers[0].Id)
            .BuildAndAddToDb(Context);
    }

    [Fact]
    public async Task DeleteOtherName_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new DeleteOtherNameCommand(int.MaxValue, _otherName.OtherName, _otherName.WhereUsed);
        await Assert.ThrowsAsync<ProducersOtherNameNotFoundException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteOtherName_UnexistingProducerOtherName_ThrowsProducersOtherNameNotFoundException()
    {
        var command =
            new DeleteOtherNameCommand(_otherName.ProducerId, Faker.Lorem.Letter(200), Faker.Lorem.Letter(200));
        await Assert.ThrowsAsync<ProducersOtherNameNotFoundException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteOtherName_Normal_Succeeds()
    {
        var command =
            new DeleteOtherNameCommand(_otherName.ProducerId, _otherName.OtherName, _otherName.WhereUsed);

        var act = () => Mediator.Send(command);
        await act.Should().NotThrowAsync();
        
        var dbOtherNames = await Context.ProducersOtherNames.AsNoTracking().ToListAsync();
        dbOtherNames.Should().HaveCount(0);
    }
}