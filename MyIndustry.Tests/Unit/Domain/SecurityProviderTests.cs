using FluentAssertions;
using MyIndustry.Domain.Provider;

namespace MyIndustry.Tests.Unit.DomainLayer;

public class SecurityProviderTests
{
    private readonly SecurityProvider _provider = new();

    [Fact]
    public void HMacSha512_Should_Return_Consistent_Hash()
    {
        var hash1 = _provider.HMacSha512("test-data", "secret-key");
        var hash2 = _provider.HMacSha512("test-data", "secret-key");

        hash1.Should().NotBeNullOrEmpty();
        hash1.Should().Be(hash2);
        hash1.Should().MatchRegex("^[a-f0-9]+$");
    }

    [Fact]
    public void HMacSha512_Should_Return_Different_Hash_For_Different_Keys()
    {
        var hash1 = _provider.HMacSha512("test-data", "key-one");
        var hash2 = _provider.HMacSha512("test-data", "key-two");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void EncryptAes256_And_DecryptAes256_Should_Roundtrip()
    {
        const string plainText = "Sensitive seller identity number 12345678901";

        var encrypted = _provider.EncryptAes256(plainText);
        var decrypted = _provider.DecryptAes256(encrypted);

        encrypted.Should().NotBe(plainText);
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void EncryptAes256_Should_Produce_Different_Output_For_Same_Input()
    {
        const string plainText = "same-value";

        var encrypted1 = _provider.EncryptAes256(plainText);
        var encrypted2 = _provider.EncryptAes256(plainText);

        encrypted1.Should().Be(encrypted2);
    }
}
