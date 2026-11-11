using System.Diagnostics.CodeAnalysis;
using Orchid.Contract.Model;
using SQLite;

namespace Orchid.Core.Domain.Entity;

/// <summary>
/// 현재 클라이언트와 페어링된 디바이스 세션 엔티티입니다.
/// </summary>
[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Cryptographic public key")]
public sealed class DeviceSession
{
    /// <summary>
    /// <see cref="DeviceSession"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public DeviceSession(
        Guid deviceId,
        string deviceName,
        DevicePlatform platform,
        byte[] publicKey,
        DateTimeOffset pairedAtUtc)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        Platform = platform;
        PublicKey = publicKey;
        PairedAtUtc = pairedAtUtc;
        LastSeenAtUtc = pairedAtUtc;
    }

    /// <summary>
    /// 기본 생성자 (SQLite ORM 역직렬화용)
    /// </summary>
    public DeviceSession()
    {
        DeviceName = string.Empty;
        PublicKey = [];
    }

    /// <summary>
    /// 디바이스 고유 식별자
    /// </summary>
    [PrimaryKey]
    public Guid DeviceId { get; init; }

    /// <summary>
    /// 디바이스 이름 (예: "MacBook Pro", "Galaxy S24")
    /// </summary>
    public string DeviceName { get; set; }

    /// <summary>
    /// 디바이스 플랫폼
    /// </summary>
    public DevicePlatform Platform { get; init; }

    /// <summary>
    /// 디바이스의 E2EE 공개키
    /// </summary>
    public byte[] PublicKey { get; init; }

    /// <summary>
    /// 페어링 수립 일시 (UTC)
    /// </summary>
    public DateTimeOffset PairedAtUtc { get; init; }

    /// <summary>
    /// 마지막 동기화 통신 일시 (UTC)
    /// </summary>
    public DateTimeOffset LastSeenAtUtc { get; set; }

    /// <summary>
    /// 디바이스 신뢰 활성화 여부
    /// </summary>
    public bool IsTrusted { get; set; } = true;
}
