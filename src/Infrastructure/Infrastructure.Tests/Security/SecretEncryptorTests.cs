using Microsoft.Extensions.Options;
using Security;
using Security.Services;

namespace Infrastructure.Tests.Security;

public class SecretEncryptorTests
{
    [Fact]
    public void Decrypt_WhenEncryptedBySameKey_ReturnsOriginalValue()
    {
        using var encryptor = CreateEncryptor();
        const string value = "supplier-password-123";

        var encrypted = encryptor.Encrypt(value);

        Assert.NotEqual(value, encrypted);
        Assert.Equal(value, encryptor.Decrypt(encrypted));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-encrypted")]
    [InlineData("v2.abc.def.ghi")]
    [InlineData("v1...")]
    [InlineData("v1.a.a.a")]
    [InlineData("v1.abc.def.ghi")]
    [InlineData("v1.!!!!.def.ghi")]
    public void TryDecrypt_WhenEncryptedValueIsMalformed_ReturnsFalse(string encrypted)
    {
        using var encryptor = CreateEncryptor();

        var result = encryptor.TryDecrypt(encrypted, out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    private static SecretEncryptor CreateEncryptor()
    {
        var options = Options.Create(
            new SecretEncryptionOptions
            {
                Secret = Convert.ToBase64String(new byte[32])
            });

        return new SecretEncryptor(options);
    }
}