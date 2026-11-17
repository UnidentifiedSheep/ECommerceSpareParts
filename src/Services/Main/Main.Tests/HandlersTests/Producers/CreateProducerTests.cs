using FluentAssertions;
using FluentValidation.TestHelper;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Producer;
using Main.Modules.Product.Application.Dtos.Producer;
using Main.Modules.Product.Application.Handlers.Producers.CreateProducer;
using Microsoft.EntityFrameworkCore;
using Tests.Abstractions.Test;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Producers;

public class CreateProducerTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{

	[Fact]
	public async Task CreateProducer_WithValidData_Succeeds()
	{
		var producer = CreateDto();
		var command = new CreateProducerCommand(producer);

		var created = await Mediator.Send(command, CancellationToken);

		var createdProducer = await Context
			.Set<Producer>()
			.AsNoTracking()
			.SingleAsync(x => x.Id == created.ProducerId, CancellationToken);

		created.ProducerId.Should().BeGreaterThan(0);

		createdProducer.Name.Should().Be(Producer.ToNormalizedName(producer.Name));
		createdProducer.Description.Should().Be(producer.Description);
	}

	[Theory]
	[InlineData("bosch")]
	[InlineData(" BOSCH ")]
	public async Task DuplicateNormalizedName_ReturnsLocalizedConflict(string name)
	{
		await new ProducerBuilder(Faker).WithName("Bosch").BuildAndAddToDb(Context, CancellationToken);

		var exception = await Assert.ThrowsAsync<DbValidationException>(() =>
			Mediator.Send(new CreateProducerCommand(CreateDto() with { Name = name }), CancellationToken));

		exception.Failures
			.Should()
			.ContainSingle(failure => failure.Error is ProducerNameAlreadyTakenException);
		(await Context.Set<Producer>().CountAsync(producer => producer.Name == "BOSCH", CancellationToken))
			.Should()
			.Be(1);
	}

	private NewProducerDto CreateDto()
		=> new()
		{
			Name = Faker.Lorem.Word() + Faker.Random.AlphaNumeric(4), Description = Faker.Lorem.Sentence()
		};
}

public class CreateProducerValidationTests : ValidationTest<CreateProducerValidation, CreateProducerCommand>
{
	[Theory]
	[InlineData("")]
	[InlineData("           ")]
	[InlineData("a")]
	public async Task CreateProducer_TooShortName_FailsValidation(string name)
	{
		var result = await ValidateAsync(
			new CreateProducerCommand(new NewProducerDto { Name = name, Description = null }));
		result.ShouldHaveValidationErrorFor(x => x.NewProducer.Name).Only();
	}

	[Fact]
	public async Task CreateProducer_TooLargeDescription_FailsValidation()
	{
		var result = await ValidateAsync(
			new CreateProducerCommand(
				new NewProducerDto { Name = "Bosch", Description = new string('x', 600) }));
		result.ShouldHaveValidationErrorFor(x => x.NewProducer.Description).Only();
	}
}
