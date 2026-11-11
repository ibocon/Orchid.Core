using System.Security.Cryptography;
using FluentAssertions;
using Orchid.Core.Cryptography;
using Xunit;

namespace Orchid.UnitTests.Core;

public sealed class AesGcmCryptoServiceTests
{
    [Fact]
    public void Encrypt_And_Decrypt_Should_Restore_Original_PlainText()
    {
        // Given
        var service = new AesGcmCryptoService();
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        var plainText = "Sensitive Clipboard Data"u8.ToArray();

        // When
        var encryptResult = service.Encrypt(plainText, key);

        // Then
        encryptResult.IsSuccess.Should().BeTrue();
        encryptResult.Value.CipherText.Should().NotBeEquivalentTo(plainText);

        var decryptResult = service.Decrypt(
            encryptResult.Value.CipherText,
            encryptResult.Value.Nonce,
            encryptResult.Value.Tag,
            key);

        decryptResult.IsSuccess.Should().BeTrue();
        decryptResult.Value.Should().BeEquivalentTo(plainText);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(16)]
    [InlineData(31)]
    [InlineData(33)]
    public void Encrypt_With_Invalid_Key_Length_Should_Fail(int keyLength)
    {
        // Given
        var service = new AesGcmCryptoService();
        byte[] invalidKey = new byte[keyLength];
        byte[] plainText = "Sample Data"u8.ToArray();

        // When
        var result = service.Encrypt(plainText, invalidKey);

        // Then
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("32 bytes");
    }

    [Fact]
    public void Decrypt_With_Tampered_Tag_Should_Fail()
    {
        // Given
        var service = new AesGcmCryptoService();
        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);
        byte[] plainText = "Sensitive Data"u8.ToArray();
        var encryptResult = service.Encrypt(plainText, key);

        byte[] tamperedTag = (byte[])encryptResult.Value.Tag.Clone();
        tamperedTag[0] ^= 0xFF;

        // When
        var decryptResult = service.Decrypt(
            encryptResult.Value.CipherText,
            encryptResult.Value.Nonce,
            tamperedTag,
            key);

        // Then
        decryptResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Decrypt_With_Invalid_Key_Should_Fail()
    {
        // Given
        var service = new AesGcmCryptoService();
        var validKey = new byte[32];
        var invalidKey = new byte[32];
        RandomNumberGenerator.Fill(validKey);
        RandomNumberGenerator.Fill(invalidKey);
        var plainText = "Confidential Information"u8.ToArray();

        var encryptResult = service.Encrypt(plainText, validKey);
        encryptResult.IsSuccess.Should().BeTrue();

        // When
        var decryptResult = service.Decrypt(
            encryptResult.Value.CipherText,
            encryptResult.Value.Nonce,
            encryptResult.Value.Tag,
            invalidKey);

        // Then
        decryptResult.IsFailure.Should().BeTrue();
    }
}
