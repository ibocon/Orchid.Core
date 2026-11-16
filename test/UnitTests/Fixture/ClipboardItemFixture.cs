using Orchid.Contract.Model;
using Orchid.Core.Domain.Common;
using Orchid.Core.Domain.Entity;

namespace Orchid.UnitTests.Core.Fixture;

/// <summary>
/// 규칙을 만족하는 <see cref="ClipboardItem"/>을 만드는 테스트 도우미입니다.
/// </summary>
internal static class ClipboardItemFixture
{
    public static Result<ClipboardItem> Create(
        Guid? id = null,
        byte[]? cipherText = null,
        byte[]? nonce = null,
        byte[]? tag = null,
        string payloadHash = "hash-001")
    {
        return ClipboardItem.Create(
            id ?? Guid.NewGuid(),
            Guid.NewGuid(),
            DevicePlatform.Windows,
            ContentType.Text,
            cipherText ?? [1, 2, 3, 4],
            nonce ?? new byte[ClipboardItem.NonceLength],
            tag ?? new byte[ClipboardItem.TagLength],
            payloadHash,
            DateTimeOffset.UtcNow);
    }

    public static ClipboardItem CreateValid()
    {
        return Create().Value;
    }
}
