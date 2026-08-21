using Abstractions.Models.Options;
using Microsoft.Extensions.Options;
using Security;
using Security.Services;

namespace Infrastructure.Tests.Security;

public class JsonSignerTests
{
    [Fact]
    public void VerifyJson_WhenTokenIsValid_ReturnsOriginalPayload()
    {
        var signer = CreateSigner();
        var payload = new TestPayload(Guid.NewGuid(), "value");

        var token = signer.Sign(payload);
        var result = signer.VerifyJson<TestPayload>(token, out var verified);

        Assert.True(result);
        Assert.Equal(payload, verified);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("invalid.token.parts")]
    [InlineData("!!!!.signature")]
    public void VerifyJson_WhenTokenIsMalformed_ReturnsFalse(string token)
    {
        var signer = CreateSigner();

        var result = signer.VerifyJson<TestPayload>(token, out var payload);

        Assert.False(result);
        Assert.Null(payload);
    }

    [Fact]
    public void VerifyJson_WhenSignedJsonDoesNotMatchType_ReturnsFalse()
    {
        var signer = CreateSigner();
        var token = signer.Sign("not a test payload");

        var result = signer.VerifyJson<TestPayload>(token, out var payload);

        Assert.False(result);
        Assert.Null(payload);
    }

    private static JsonSigner CreateSigner()
    {
        return new JsonSigner(
            Options.Create(
                new SecretEncryptionOptions
                {
                    Secret = "json-signer-test-secret"
                }),
            new ProjectJsonOptions());
    }

    private sealed record TestPayload(Guid Id, string Value);
}
