using Application.Common.Models;
using FluentAssertions;
using Main.Entities.Product.Producer;
using Main.Modules.Product.Application.Dtos.Producer;
using Main.Modules.Product.Application.Handlers.Producers.EditProducer;
using Microsoft.EntityFrameworkCore;
using Tests.Abstractions.Test;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class EditProducerTests : IntegrationTest
{
	public EditProducerTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<ProducerTestContext>();

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

	[Fact]
	public async Task EditProducer_NoValuesSet_Succeeds()
	{
		var producer = GetFirstProducer();

		var act = () => Mediator.Send(new EditProducerCommand(producer.Id, new PatchProducerDto()));

		await act.Should().NotThrowAsync();

		var dbProducer = await Context
			.Set<Producer>()
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Id == producer.Id, CancellationToken);
		dbProducer.Should().NotBeNull();

		dbProducer.Name.Should().Be(producer.Name);
		dbProducer.Description.Should().Be(producer.Description);
	}

	[Fact]
	public async Task EditProducer_Normal_Succeeds()
	{
		var producer = GetFirstProducer();
		var model = new PatchProducerDto
		{
			Name = new PatchField<string>
			{
				IsSet = true, Value = Faker.Lorem.Letter(30)
			},
			Description =
			{
				IsSet = true, Value = Faker.Lorem.Letter(30)
			}
		};
		var command = new EditProducerCommand(producer.Id, model);
		var edited = await Mediator.Send(command, CancellationToken);

		var dbProducer = await Context
			.Set<Producer>()
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Id == producer.Id, CancellationToken);
		dbProducer.Should().NotBeNull();

		edited.ProducerId.Should().Be(producer.Id);
		dbProducer.Name.Should().Be(Producer.ToNormalizedName(model.Name.Value));
		dbProducer.Description.Should().Be(model.Description.Value);
	}

	private Producer GetFirstProducer() => TestContext.Producers[0];
}

public class EditProducerValidationTests : ValidationTest<EditProducerValidation, EditProducerCommand>
{
	[Theory]
	[InlineData("tooBig")]
	[InlineData("в")]
	public async Task EditProducer_TooLargeName_FailsValidation(string? name)
	{
		if (name == "tooBig")
			name = new string('x', 200);
		var model = new PatchProducerDto
		{
			Name = new PatchField<string>
			{
				IsSet = true, Value = name
			}
		};
		var command = new EditProducerCommand(1, model);
		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor("Producer.Name.Value");
	}

	[Fact]
	public async Task EditProducer_IsSetButNullValue_FailsValidation()
	{
		var model = new PatchProducerDto
		{
			Name = new PatchField<string>
			{
				IsSet = true, Value = null
			}
		};
		var command = new EditProducerCommand(1, model);
		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor("Producer.Name.Value");
	}

	[Fact]
	public async Task EditProducer_DescriptionTooLarge_FailsValidation()
	{
		var model = new PatchProducerDto
		{
			Description = new PatchField<string?>
			{
				IsSet = true, Value = new string('x', 1000)
			}
		};
		var command = new EditProducerCommand(1, model);
		var result = await ValidateAsync(command);
		result.ShouldHaveValidationErrorFor("Producer.Description.Value");
	}
}
