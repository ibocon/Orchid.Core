using System.Diagnostics.CodeAnalysis;
using Orchid.Contract.Model;
using Orchid.Core.Domain.Entity;
using SQLite;

namespace Orchid.Core.Storage;

/// <summary>
/// <see cref="DeviceSession"/>의 SQLite 저장 형태입니다. 기존 테이블 이름과 컬럼 이름을 유지합니다.
/// </summary>
[Table("DeviceSession")]
[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "SQLite blob column mapping")]
internal sealed class DeviceSessionRow
{
    [PrimaryKey]
    public Guid DeviceId { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public DevicePlatform Platform { get; set; }

    public byte[] PublicKey { get; set; } = [];

    public DateTimeOffset PairedAtUtc { get; set; }

    public DateTimeOffset LastSeenAtUtc { get; set; }

    public bool IsTrusted { get; set; }

    internal static DeviceSessionRow From(DeviceSession session)
    {
        return new DeviceSessionRow
        {
            DeviceId = session.DeviceId,
            DeviceName = session.DeviceName,
            Platform = session.Platform,
            PublicKey = session.PublicKey,
            PairedAtUtc = session.PairedAtUtc,
            LastSeenAtUtc = session.LastSeenAtUtc,
            IsTrusted = session.IsTrusted,
        };
    }

    internal DeviceSession ToDomain()
    {
        return DeviceSession.Rehydrate(DeviceId, DeviceName, Platform, PublicKey, PairedAtUtc, LastSeenAtUtc, IsTrusted);
    }
}
