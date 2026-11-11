using System.Security.Cryptography;

namespace Orchid.Core.Domain.ValueObject;

/// <summary>
/// 클립보드 페이로드의 중복 여부를 감지하기 위한 SHA-256 불변 해시 값 객체입니다.
/// </summary>
public sealed record PayloadHash
{
    private PayloadHash(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 16진수 문자열 형식의 SHA-256 해시 값입니다.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 바이트 배열로부터 SHA-256 해시 값 객체를 생성합니다.
    /// </summary>
    /// <param name="data">해시할 원본 데이터</param>
    /// <returns>생성된 <see cref="PayloadHash"/> 인스턴스</returns>
    public static PayloadHash FromBytes(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        byte[] hashBytes = SHA256.HashData(data);
        return new PayloadHash(Convert.ToHexStringLower(hashBytes));
    }

    /// <summary>
    /// 문자열로부터 SHA-256 해시 값 객체를 생성합니다.
    /// </summary>
    /// <param name="text">해시할 원본 텍스트</param>
    /// <returns>생성된 <see cref="PayloadHash"/> 인스턴스</returns>
    public static PayloadHash FromString(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return FromBytes(System.Text.Encoding.UTF8.GetBytes(text));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}
