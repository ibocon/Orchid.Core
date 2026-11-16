using System.Diagnostics.CodeAnalysis;
using Orchid.Contract.Model;
using Orchid.Core.Domain.Entity;
using SQLite;

namespace Orchid.Core.Storage;

/// <summary>
/// <see cref="ClipboardItem"/>의 SQLite 저장 형태입니다. 기존 테이블 이름과 컬럼 이름을 유지합니다.
/// </summary>
[Table("ClipboardItem")]
[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "SQLite blob column mapping")]
internal sealed class ClipboardItemRow
{
    [PrimaryKey]
    public Guid Id { get; set; }

    public Guid SourceDeviceId { get; set; }

    public DevicePlatform Platform { get; set; }

    public ContentType ContentType { get; set; }

    public byte[] CipherText { get; set; } = [];

    public byte[] Nonce { get; set; } = [];

    public byte[] Tag { get; set; } = [];

    [Indexed]
    public string PayloadHash { get; set; } = string.Empty;

    public bool IsPinned { get; set; }

    public bool IsDeleted { get; set; }

    [Indexed]
    public DateTimeOffset CreatedAtUtc { get; set; }

    internal static ClipboardItemRow From(ClipboardItem item)
    {
        return new ClipboardItemRow
        {
            Id = item.Id,
            SourceDeviceId = item.SourceDeviceId,
            Platform = item.Platform,
            ContentType = item.ContentType,
            CipherText = item.CipherText,
            Nonce = item.Nonce,
            Tag = item.Tag,
            PayloadHash = item.PayloadHash,
            IsPinned = item.IsPinned,
            IsDeleted = item.IsDeleted,
            CreatedAtUtc = item.CreatedAtUtc,
        };
    }

    internal ClipboardItem ToDomain()
    {
        return ClipboardItem.Rehydrate(
            Id,
            SourceDeviceId,
            Platform,
            ContentType,
            CipherText,
            Nonce,
            Tag,
            PayloadHash,
            CreatedAtUtc,
            IsPinned,
            IsDeleted);
    }
}
