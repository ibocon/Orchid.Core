using System.Security.Cryptography;
using Orchid.Core.Domain.Common;

namespace Orchid.Core.Sync.Relay;

/// <summary>
/// 디바이스 간 E2EE 공유 키와 서버가 보는 페어링 식별자를 묶은 값 객체입니다.
/// </summary>
public sealed class PairingKey
{
    private const string Prefix = "orchid-pair:v1:";
    private const int KeyLength = 32;

    private readonly byte[] _key;

    private PairingKey(byte[] key)
    {
        _key = key;
        PairingId = Convert.ToHexString(SHA256.HashData(key));
    }

    /// <summary>
    /// 서버에 전달하는 키 해시 식별자를 가져옵니다.
    /// </summary>
    public string PairingId { get; }

    /// <summary>
    /// 새 난수 페어링 키를 생성합니다.
    /// </summary>
    /// <returns>생성된 페어링 키</returns>
    public static PairingKey Generate()
    {
        return new PairingKey(RandomNumberGenerator.GetBytes(KeyLength));
    }

    /// <summary>
    /// QR 또는 수동 입력 문자열을 파싱합니다.
    /// </summary>
    /// <param name="payload">직렬화된 페어링 키 문자열</param>
    /// <returns>파싱 결과</returns>
    public static Result<PairingKey> Parse(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload) || !payload.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return Result<PairingKey>.Failure(Error.Validation("Pairing payload has an unsupported format."));
        }

        string base64 = payload[Prefix.Length..].Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '=');

        byte[] key;
        try
        {
            key = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return Result<PairingKey>.Failure(Error.Validation("Pairing key is not valid base64url."));
        }

        return key.Length == KeyLength
            ? Result<PairingKey>.Success(new PairingKey(key))
            : Result<PairingKey>.Failure(Error.Validation("Pairing key must be 32 bytes."));
    }

    /// <summary>
    /// 암호화·복호화에 사용할 키 바이트의 사본을 반환합니다.
    /// </summary>
    /// <returns>키 바이트 사본</returns>
    public byte[] GetKeyBytes()
    {
        return (byte[])_key.Clone();
    }

    /// <summary>
    /// QR과 수동 입력에 쓰는 문자열로 직렬화합니다.
    /// </summary>
    /// <returns>직렬화된 문자열</returns>
    public string ToPayload()
    {
        return Prefix + Convert.ToBase64String(_key).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
