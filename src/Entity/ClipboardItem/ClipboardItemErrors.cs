using Orchid.Core.Domain.Common;

namespace Orchid.Core.Domain.Entity;

/// <summary>
/// <see cref="ClipboardItem"/>의 규칙 위반을 나타내는 도메인 오류 모음입니다.
/// </summary>
public static class ClipboardItemErrors
{
    /// <summary>
    /// 항목 식별자가 비어 있습니다.
    /// </summary>
    public static readonly Error IdRequired = new("ClipboardItem.IdRequired", "Clipboard item id cannot be empty.");

    /// <summary>
    /// 암호화된 데이터가 비어 있습니다.
    /// </summary>
    public static readonly Error CipherTextRequired = new("ClipboardItem.CipherTextRequired", "Cipher text cannot be empty.");

    /// <summary>
    /// 논스 길이가 규칙과 다릅니다.
    /// </summary>
    public static readonly Error InvalidNonceLength = new("ClipboardItem.InvalidNonceLength", "Nonce must be 12 bytes.");

    /// <summary>
    /// 인증 태그 길이가 규칙과 다릅니다.
    /// </summary>
    public static readonly Error InvalidTagLength = new("ClipboardItem.InvalidTagLength", "Tag must be 16 bytes.");

    /// <summary>
    /// 중복 방지용 해시가 비어 있습니다.
    /// </summary>
    public static readonly Error PayloadHashRequired = new("ClipboardItem.PayloadHashRequired", "Payload hash cannot be blank.");

    /// <summary>
    /// 삭제된 항목은 고정할 수 없습니다.
    /// </summary>
    public static readonly Error DeletedItemCannotBePinned = new("ClipboardItem.DeletedItemCannotBePinned", "A deleted item cannot be pinned.");
}
