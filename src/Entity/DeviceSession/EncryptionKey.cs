using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Orchid.Core.Domain.Common;

namespace Orchid.Core.Domain.ValueObject;

/// <summary>
/// E2EE AES-256-GCM 대칭키를 캡슐화하는 메모리 보안 값 객체입니다.
/// </summary>
public sealed class EncryptionKey
{
    private readonly byte[] _keyBytes;

    private EncryptionKey(byte[] keyBytes)
    {
        _keyBytes = keyBytes;
    }

    /// <summary>
    /// 키 바이트 배열의 읽기 전용 복사본을 반환합니다.
    /// </summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Cryptographic key bytes clone")]
    public byte[] KeyBytes => (byte[])_keyBytes.Clone();

    /// <summary>
    /// 바이트 배열로부터 256비트(32바이트) 암호화 키를 생성합니다.
    /// </summary>
    /// <param name="keyBytes">32바이트 길이의 키 데이터</param>
    /// <returns>유효성 검증 결과를 포함하는 <see cref="Result{EncryptionKey}"/></returns>
    public static Result<EncryptionKey> Create(byte[] keyBytes)
    {
        if (keyBytes == null || keyBytes.Length != 32)
        {
            return Result<EncryptionKey>.Failure("Key must be exactly 256 bits (32 bytes).");
        }

        return Result<EncryptionKey>.Success(new EncryptionKey((byte[])keyBytes.Clone()));
    }

    /// <summary>
    /// CSPRNG를 사용하여 새로운 256비트 암호화 키를 무작위로 생성합니다.
    /// </summary>
    /// <returns>새로 생성된 <see cref="EncryptionKey"/></returns>
    public static EncryptionKey Generate()
    {
        byte[] bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return new EncryptionKey(bytes);
    }
}
