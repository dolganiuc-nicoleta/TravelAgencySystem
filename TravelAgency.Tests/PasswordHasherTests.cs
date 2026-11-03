using System.Security.Cryptography;
using System.Text;
using TravelAgency.Core.Services;

namespace TravelAgency.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ThenVerify_AcceptsCorrectPassword()
    {
        var hash = PasswordHasher.Hash("Secret123!");

        Assert.True(PasswordHasher.Verify("Secret123!", hash));
    }

    [Fact]
    public void Verify_RejectsWrongPassword()
    {
        var hash = PasswordHasher.Hash("Secret123!");

        Assert.False(PasswordHasher.Verify("secret123!", hash));
    }

    [Fact]
    public void Hash_UsesRandomSalt_SoEqualPasswordsProduceDifferentHashes()
    {
        Assert.NotEqual(PasswordHasher.Hash("Secret123!"), PasswordHasher.Hash("Secret123!"));
    }

    [Fact]
    public void Hash_ProducesCurrentFormat_ThatDoesNotNeedRehash()
    {
        var hash = PasswordHasher.Hash("Secret123!");

        Assert.StartsWith("pbkdf2-sha256$", hash);
        Assert.False(PasswordHasher.NeedsRehash(hash));
    }

    [Fact]
    public void Verify_AcceptsLegacyUnsaltedSha256Hash_AndFlagsItForRehash()
    {
        var legacy = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("Secret123!")));

        Assert.True(PasswordHasher.Verify("Secret123!", legacy));
        Assert.False(PasswordHasher.Verify("Wrong", legacy));
        Assert.True(PasswordHasher.NeedsRehash(legacy));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("pbkdf2-sha256$abc$salt$hash")]
    [InlineData("pbkdf2-sha256$1000$!!!$!!!")]
    public void Verify_ReturnsFalse_ForMalformedHashes(string stored)
    {
        Assert.False(PasswordHasher.Verify("Secret123!", stored));
    }

    [Fact]
    public void Hash_RejectsEmptyPassword()
    {
        Assert.Throws<ArgumentException>(() => PasswordHasher.Hash(" "));
    }
}
