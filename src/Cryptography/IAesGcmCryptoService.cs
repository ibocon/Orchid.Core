using Orchid.Core.Domain.Common;

namespace Orchid.Core.Cryptography;

/// <summary>
/// E2EE AES-256-GCM 대칭키 암호화 및 복호화 서비스 인터페이스입니다.
/// </summary>
public interface IAesGcmCryptoService
{
    /// <summary>
    /// 평문 바이트 데이터를 주어진 256비트 대칭키로 AES-GCM 암호화합니다.
    /// </summary>
    /// <param name="plainText">암호화할 평문 바이트 배열</param>
    /// <param name="key">32바이트 길이의 256비트 대칭키</param>
    /// <returns>암호문, 논스(12바이트), 태그(16바이트) 튜플을 포함하는 <see cref="Result"/></returns>
    Result<(byte[] CipherText, byte[] Nonce, byte[] Tag)> Encrypt(byte[] plainText, byte[] key);

    /// <summary>
    /// 암호화된 데이터를 논스 및 인증 태그를 사용하여 복호화합니다.
    /// </summary>
    /// <param name="cipherText">복호화할 암호문 바이트 배열</param>
    /// <param name="nonce">암호화에 사용된 논스(12바이트)</param>
    /// <param name="tag">암호화에 사용된 인증 태그(16바이트)</param>
    /// <param name="key">32바이트 길이의 256비트 대칭키</param>
    /// <returns>복호화된 평문 바이트 배열을 포함하는 <see cref="Result"/></returns>
    Result<byte[]> Decrypt(byte[] cipherText, byte[] nonce, byte[] tag, byte[] key);
}
