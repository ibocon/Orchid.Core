using System.Security.Cryptography;
using Orchid.Core.Domain.Common;

namespace Orchid.Core.Cryptography;

/// <summary>
/// .NET BCL <see cref="AesGcm"/> 기반의 고성능 하드웨어 가속 E2EE 암호화 서비스 구현체입니다.
/// </summary>
public sealed class AesGcmCryptoService : IAesGcmCryptoService
{
    /// <summary>
    /// AES-GCM 논스 바이트 크기 (12바이트 / 96비트)
    /// </summary>
    public const int NonceSize = 12;

    /// <summary>
    /// AES-GCM 인증 태그 바이트 크기 (16바이트 / 128비트)
    /// </summary>
    public const int TagSize = 16;

    /// <inheritdoc />
    public Result<(byte[] CipherText, byte[] Nonce, byte[] Tag)> Encrypt(byte[] plainText, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length != 32)
        {
            return Result<(byte[], byte[], byte[])>.Failure("Key must be 256 bits (32 bytes).");
        }

        try
        {
            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            byte[] cipherText = new byte[plainText.Length];
            byte[] tag = new byte[TagSize];

            using AesGcm aes = new(key, TagSize);
            aes.Encrypt(nonce, plainText, cipherText, tag);

            return Result<(byte[], byte[], byte[])>.Success((cipherText, nonce, tag));
        }
        catch (CryptographicException ex)
        {
            return Result<(byte[], byte[], byte[])>.Failure($"Encryption failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Result<byte[]> Decrypt(byte[] cipherText, byte[] nonce, byte[] tag, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(cipherText);
        ArgumentNullException.ThrowIfNull(nonce);
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length != 32)
        {
            return Result<byte[]>.Failure("Key must be 256 bits (32 bytes).");
        }

        if (nonce.Length != NonceSize)
        {
            return Result<byte[]>.Failure($"Nonce must be {NonceSize} bytes.");
        }

        if (tag.Length != TagSize)
        {
            return Result<byte[]>.Failure($"Tag must be {TagSize} bytes.");
        }

        try
        {
            byte[] plainText = new byte[cipherText.Length];
            using AesGcm aes = new(key, TagSize);
            aes.Decrypt(nonce, cipherText, tag, plainText);

            return Result<byte[]>.Success(plainText);
        }
        catch (CryptographicException ex)
        {
            return Result<byte[]>.Failure($"Decryption failed (authentication tag mismatch or corrupted data): {ex.Message}");
        }
    }
}
