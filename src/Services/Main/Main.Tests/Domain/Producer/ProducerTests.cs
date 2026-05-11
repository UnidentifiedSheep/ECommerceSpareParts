using FluentAssertions;
using ProducerDomain = Main.Entities.Producer.Producer;

namespace Tests.Domain.Producer;

public class ProducerTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var producer = ProducerDomain.Create(
            "sony",
            "desc",
            "/img.png");

        producer.Name.Should().Be("Sony");
        producer.Description.Should().Be("desc");
        producer.ImagePath.Should().Be("/img.png");
    }

    [Fact]
    public void Create_WithoutOptionalFields_Succeeds()
    {
        var producer = ProducerDomain.Create("sony");

        producer.Name.Should().Be("Sony");
        producer.Description.Should().BeNull();
        producer.ImagePath.Should().BeNull();
    }

    [Fact]
    public void SetName_ChangesValue()
    {
        var producer = ProducerDomain.Create("sony");

        producer.SetName("samsung");

        producer.Name.Should().Be("Samsung");
    }

    [Fact]
    public void SetDescription_TrimsAndNormalizes()
    {
        var producer = ProducerDomain.Create("sony");

        producer.SetDescription("   test desc   ");

        producer.Description.Should().Be("test desc");
    }

    [Fact]
    public void SetDescription_EmptyBecomesNull()
    {
        var producer = ProducerDomain.Create("sony");

        producer.SetDescription("   ");

        producer.Description.Should().BeNull();
    }

    [Fact]
    public void SetImagePath_Trims()
    {
        var producer = ProducerDomain.Create("sony");

        producer.SetImagePath("   /img.png   ");

        producer.ImagePath.Should().Be("/img.png");
    }

    [Fact]
    public void SetImagePath_EmptyBecomesNull()
    {
        var producer = ProducerDomain.Create("sony");

        producer.SetImagePath("   ");

        producer.ImagePath.Should().BeNull();
    }
}