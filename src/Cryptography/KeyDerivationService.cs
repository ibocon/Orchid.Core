using System.Security.Cryptography;
using System.Text;

namespace Orchid.Core.Cryptography;

/// <summary>
/// 사용자 패스프레이즈 또는 마스터 키로부터 256비트(32바이트) 암호화 키를 파생하는 서비스입니다.
/// </summary>
public static class KeyDerivationService
{
    private const int Iterations = 100_000;
    private const int KeySize = 32;

    /// <summary>
    /// 패스프레이즈와 솔트(Salt)를 사용하여 32바이트 AES-256 키를 유도합니다.
    /// </summary>
    /// <param name="passphrase">사용자 마스터 비밀번호</param>
    /// <param name="salt">솔트 바이트 배열(최소 16바이트 권장)</param>
    /// <returns>32바이트 파생 키</returns>
    public static byte[] DeriveKey(string passphrase, byte[] salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passphrase);
        ArgumentNullException.ThrowIfNull(salt);

        byte[] passwordBytes = Encoding.UTF8.GetBytes(passphrase);
        return Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
    }
}
