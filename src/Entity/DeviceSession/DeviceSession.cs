using System.Diagnostics.CodeAnalysis;
using Orchid.Contract.Model;
using Orchid.Core.Domain.Common;

namespace Orchid.Core.Domain.Entity;

/// <summary>
/// 현재 클라이언트와 페어링된 디바이스 세션 엔티티입니다.
/// </summary>
[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Cryptographic public key")]
public sealed class DeviceSession
{
    private DeviceSession(
        Guid deviceId,
        string deviceName,
        DevicePlatform platform,
        byte[] publicKey,
        DateTimeOffset pairedAtUtc,
        DateTimeOffset lastSeenAtUtc,
        bool isTrusted)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        Platform = platform;
        PublicKey = publicKey;
        PairedAtUtc = pairedAtUtc;
        LastSeenAtUtc = lastSeenAtUtc;
        IsTrusted = isTrusted;
    }

    /// <summary>
    /// 디바이스 고유 식별자
    /// </summary>
    public Guid DeviceId { get; }

    /// <summary>
    /// 디바이스 이름 (예: "MacBook Pro", "Galaxy S24")
    /// </summary>
    public string DeviceName { get; private set; }

    /// <summary>
    /// 디바이스 플랫폼
    /// </summary>
    public DevicePlatform Platform { get; }

    /// <summary>
    /// 디바이스의 E2EE 공개키
    /// </summary>
    public byte[] PublicKey { get; }

    /// <summary>
    /// 페어링 수립 일시 (UTC)
    /// </summary>
    public DateTimeOffset PairedAtUtc { get; }

    /// <summary>
    /// 마지막 동기화 통신 일시 (UTC)
    /// </summary>
    public DateTimeOffset LastSeenAtUtc { get; private set; }

    /// <summary>
    /// 디바이스 신뢰 활성화 여부
    /// </summary>
    public bool IsTrusted { get; private set; }

    /// <summary>
    /// 규칙을 검증해 새 디바이스 세션을 만듭니다. 새 세션은 신뢰 상태이며 마지막 통신 시각은 페어링 시각입니다.
    /// </summary>
    /// <param name="deviceId">디바이스 고유 식별자</param>
    /// <param name="deviceName">디바이스 이름</param>
    /// <param name="platform">디바이스 플랫폼</param>
    /// <param name="publicKey">E2EE 공개키</param>
    /// <param name="pairedAtUtc">페어링 수립 일시 (UTC)</param>
    /// <returns>검증을 통과하면 성공 결과, 아니면 검증 오류</returns>
    public static Result<DeviceSession> Create(
        Guid deviceId,
        string deviceName,
        DevicePlatform platform,
        byte[] publicKey,
        DateTimeOffset pairedAtUtc)
    {
        if (deviceId == Guid.Empty)
        {
            return Result<DeviceSession>.Failure(Error.Validation("Device id cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(deviceName))
        {
            return Result<DeviceSession>.Failure(Error.Validation("Device name cannot be blank."));
        }

        if (publicKey is null || publicKey.Length == 0)
        {
            return Result<DeviceSession>.Failure(Error.Validation("Public key cannot be empty."));
        }

        DeviceSession session = new(deviceId, deviceName, platform, publicKey, pairedAtUtc, pairedAtUtc, isTrusted: true);
        return Result<DeviceSession>.Success(session);
    }

    /// <summary>
    /// 저장소에서 읽은 값으로 디바이스 세션을 재구성합니다. 검증을 다시 실행하지 않습니다.
    /// </summary>
    internal static DeviceSession Rehydrate(
        Guid deviceId,
        string deviceName,
        DevicePlatform platform,
        byte[] publicKey,
        DateTimeOffset pairedAtUtc,
        DateTimeOffset lastSeenAtUtc,
        bool isTrusted)
    {
        DeviceSession session = new(deviceId, deviceName, platform, publicKey, pairedAtUtc, lastSeenAtUtc, isTrusted);
        return session;
    }

    /// <summary>
    /// 디바이스 이름을 바꿉니다. 비어 있는 이름은 거부합니다.
    /// </summary>
    /// <param name="deviceName">새 디바이스 이름</param>
    /// <returns>변경에 성공하면 성공 결과, 이름이 비어 있으면 실패 결과</returns>
    public Result Rename(string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            return Result.Failure(Error.Validation("Device name cannot be blank."));
        }

        DeviceName = deviceName;
        return Result.Success();
    }

    /// <summary>
    /// 마지막 통신 시각을 갱신합니다. 이전 시각으로는 되돌리지 않습니다.
    /// </summary>
    /// <param name="seenAtUtc">통신 일시 (UTC)</param>
    public void MarkSeen(DateTimeOffset seenAtUtc)
    {
        if (seenAtUtc > LastSeenAtUtc)
        {
            LastSeenAtUtc = seenAtUtc;
        }
    }

    /// <summary>
    /// 디바이스를 신뢰 상태로 되돌립니다.
    /// </summary>
    public void Trust()
    {
        IsTrusted = true;
    }

    /// <summary>
    /// 디바이스의 신뢰를 해제합니다.
    /// </summary>
    public void Revoke()
    {
        IsTrusted = false;
    }
}
