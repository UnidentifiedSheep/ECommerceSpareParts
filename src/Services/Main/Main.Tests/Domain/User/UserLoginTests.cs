using FluentAssertions;
using Main.Entities.DomainEvents.User;
using Main.Entities.User;

namespace Tests.Domain.User;

public class UserLoginTests
{
    [Fact]
    public void Login_UpdatesLastLoginAndAddsEventWithRequestContext()
    {
        var user = Main.Entities.User.User.Create(
            "test-user",
            "password-hash");
        const string ipAddress = "192.0.2.15";
        const string userAgent = "Mozilla/5.0 Chrome/138.0.0.0 Windows";
        var before = DateTime.UtcNow;

        user.Login(ipAddress, userAgent);

        var after = DateTime.UtcNow;
        user.LastLoginAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        var @event = user.FlushDomainEvents()
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeOfType<UserLoggedInDomainEvent>()
            .Which;

        @event.UserId.Should().Be(user.Id);
        @event.OccurredAtUtc.Should().Be(user.LastLoginAt);
        @event.IpAddress.Should().Be(ipAddress);
        @event.UserAgent.Should().Be(userAgent);
    }
}
