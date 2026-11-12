using System.Security.Cryptography;
using FluentAssertions;
using Orchid.Core.Cryptography;
using Orchid.Core.Domain.Common;
using Xunit;

namespace Orchid.UnitTests.Core;

public sealed class AesGcmCryptoServiceTest
{
    [Fact]
    public void EncryptAndDecryptShouldRestoreOriginalPlainText()
    {
        // Given
        AesGcmCryptoService service = new AesGcmCryptoService();
        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);
        byte[] plainText = "Sensitive Clipboard Data"u8.ToArray();

        // When
        Result<(byte[] CipherText, byte[] Nonce, byte[] Tag)> encryptResult = service.Encrypt(plainText, key);

        // Then
        encryptResult.IsSuccess.Should().BeTrue();
        encryptResult.Value.CipherText.Should().NotBeEquivalentTo(plainText);

        Result<byte[]> decryptResult = service.Decrypt(
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
    public void EncryptWithInvalidKeyLengthShouldFail(int keyLength)
    {
        // Given
        AesGcmCryptoService service = new AesGcmCryptoService();
        byte[] invalidKey = new byte[keyLength];
        byte[] plainText = "Sample Data"u8.ToArray();

        // When
        Result<(byte[] CipherText, byte[] Nonce, byte[] Tag)> result = service.Encrypt(plainText, invalidKey);

        // Then
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("32 bytes");
    }

    [Fact]
    public void DecryptWithTamperedTagShouldFail()
    {
        // Given
        AesGcmCryptoService service = new AesGcmCryptoService();
        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);
        byte[] plainText = "Sensitive Data"u8.ToArray();
        Result<(byte[] CipherText, byte[] Nonce, byte[] Tag)> encryptResult = service.Encrypt(plainText, key);

        byte[] tamperedTag = (byte[])encryptResult.Value.Tag.Clone();
        tamperedTag[0] ^= 0xFF;

        // When
        Result<byte[]> decryptResult = service.Decrypt(
            encryptResult.Value.CipherText,
            encryptResult.Value.Nonce,
            tamperedTag,
            key);

        // Then
        decryptResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void DecryptWithInvalidKeyShouldFail()
    {
        // Given
        AesGcmCryptoService service = new AesGcmCryptoService();
        byte[] validKey = new byte[32];
        byte[] invalidKey = new byte[32];
        RandomNumberGenerator.Fill(validKey);
        RandomNumberGenerator.Fill(invalidKey);
        byte[] plainText = "Confidential Information"u8.ToArray();

        Result<(byte[] CipherText, byte[] Nonce, byte[] Tag)> encryptResult = service.Encrypt(plainText, validKey);
        encryptResult.IsSuccess.Should().BeTrue();

        // When
        Result<byte[]> decryptResult = service.Decrypt(
            encryptResult.Value.CipherText,
            encryptResult.Value.Nonce,
            encryptResult.Value.Tag,
            invalidKey);

        // Then
        decryptResult.IsFailure.Should().BeTrue();
    }
}
