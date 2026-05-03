using FluentAssertions;
using Main.Entities.Producer.ValueObjects;

using ProducerDomain = Main.Entities.Producer.Producer;

namespace Main.Tests.Domain.Producer;

public class ProducerTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var producer = ProducerDomain.Create(
            new Name("sony"),
            "desc",
            "/img.png");

        producer.Name.Value.Should().Be("Sony");
        producer.Description.Should().Be("desc");
        producer.ImagePath.Should().Be("/img.png");
    }

    [Fact]
    public void Create_WithoutOptionalFields_Succeeds()
    {
        var producer = ProducerDomain.Create(new Name("sony"));

        producer.Name.Value.Should().Be("Sony");
        producer.Description.Should().BeNull();
        producer.ImagePath.Should().BeNull();
    }

    [Fact]
    public void SetName_ChangesValue()
    {
        var producer = ProducerDomain.Create(new Name("sony"));

        producer.SetName(new Name("samsung"));

        producer.Name.Value.Should().Be("Samsung");
    }

    [Fact]
    public void SetDescription_TrimsAndNormalizes()
    {
        var producer = ProducerDomain.Create(new Name("sony"));

        producer.SetDescription("   test desc   ");

        producer.Description.Should().Be("test desc");
    }

    [Fact]
    public void SetDescription_EmptyBecomesNull()
    {
        var producer = ProducerDomain.Create(new Name("sony"));

        producer.SetDescription("   ");

        producer.Description.Should().BeNull();
    }

    [Fact]
    public void SetImagePath_Trims()
    {
        var producer = ProducerDomain.Create(new Name("sony"));

        producer.SetImagePath("   /img.png   ");

        producer.ImagePath.Should().Be("/img.png");
    }

    [Fact]
    public void SetImagePath_EmptyBecomesNull()
    {
        var producer = ProducerDomain.Create(new Name("sony"));

        producer.SetImagePath("   ");

        producer.ImagePath.Should().BeNull();
    }
}