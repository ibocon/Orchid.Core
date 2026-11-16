using System.Diagnostics.CodeAnalysis;
using Orchid.Contract.Model;
using Orchid.Core.Domain.Common;

namespace Orchid.Core.Domain.Entity;

/// <summary>
/// 로컬 저장소 및 도메인 로직에서 취급하는 클립보드 항목 엔티티입니다.
/// </summary>
[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Cryptographic binary payload exposed to the UI")]
public sealed class ClipboardItem
{
    /// <summary>
    /// AES-GCM 논스 길이(바이트)입니다.
    /// </summary>
    public const int NonceLength = 12;

    /// <summary>
    /// AES-GCM 인증 태그 길이(바이트)입니다.
    /// </summary>
    public const int TagLength = 16;

    private ClipboardItem(
        Guid id,
        Guid sourceDeviceId,
        DevicePlatform platform,
        ContentType contentType,
        byte[] cipherText,
        byte[] nonce,
        byte[] tag,
        string payloadHash,
        DateTimeOffset createdAtUtc,
        bool isPinned,
        bool isDeleted)
    {
        Id = id;
        SourceDeviceId = sourceDeviceId;
        Platform = platform;
        ContentType = contentType;
        CipherText = cipherText;
        Nonce = nonce;
        Tag = tag;
        PayloadHash = payloadHash;
        CreatedAtUtc = createdAtUtc;
        IsPinned = isPinned;
        IsDeleted = isDeleted;
    }

    /// <summary>
    /// 항목 고유 식별자
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 발신 디바이스 식별자
    /// </summary>
    public Guid SourceDeviceId { get; }

    /// <summary>
    /// 발신 플랫폼
    /// </summary>
    public DevicePlatform Platform { get; }

    /// <summary>
    /// 콘텐츠 형식
    /// </summary>
    public ContentType ContentType { get; }

    /// <summary>
    /// 암호화된 데이터 바이트 배열
    /// </summary>
    public byte[] CipherText { get; }

    /// <summary>
    /// 암호화에 사용된 논스(12바이트)
    /// </summary>
    public byte[] Nonce { get; }

    /// <summary>
    /// 인증 태그(16바이트)
    /// </summary>
    public byte[] Tag { get; }

    /// <summary>
    /// 중복 방지용 해시
    /// </summary>
    public string PayloadHash { get; }

    /// <summary>
    /// 항목 생성 일시 (UTC)
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>
    /// 즐겨찾기 고정 여부
    /// </summary>
    public bool IsPinned { get; private set; }

    /// <summary>
    /// 삭제 여부 (소프트 삭제)
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// 규칙을 검증해 새 항목을 만듭니다. 원격 수신 항목은 발신 디바이스를 알 수 없어 <paramref name="sourceDeviceId"/>에 <see cref="Guid.Empty"/>를 허용합니다.
    /// </summary>
    /// <param name="id">항목 고유 식별자</param>
    /// <param name="sourceDeviceId">발신 디바이스 식별자</param>
    /// <param name="platform">발신 플랫폼</param>
    /// <param name="contentType">콘텐츠 형식</param>
    /// <param name="cipherText">암호화된 데이터</param>
    /// <param name="nonce">논스(12바이트)</param>
    /// <param name="tag">인증 태그(16바이트)</param>
    /// <param name="payloadHash">중복 방지용 해시</param>
    /// <param name="createdAtUtc">생성 일시 (UTC)</param>
    /// <returns>검증을 통과하면 성공 결과, 아니면 규칙 위반 오류</returns>
    public static Result<ClipboardItem> Create(
        Guid id,
        Guid sourceDeviceId,
        DevicePlatform platform,
        ContentType contentType,
        byte[] cipherText,
        byte[] nonce,
        byte[] tag,
        string payloadHash,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            return Result<ClipboardItem>.Failure(ClipboardItemErrors.IdRequired);
        }

        if (cipherText is null || cipherText.Length == 0)
        {
            return Result<ClipboardItem>.Failure(ClipboardItemErrors.CipherTextRequired);
        }

        if (nonce is null || nonce.Length != NonceLength)
        {
            return Result<ClipboardItem>.Failure(ClipboardItemErrors.InvalidNonceLength);
        }

        if (tag is null || tag.Length != TagLength)
        {
            return Result<ClipboardItem>.Failure(ClipboardItemErrors.InvalidTagLength);
        }

        if (string.IsNullOrWhiteSpace(payloadHash))
        {
            return Result<ClipboardItem>.Failure(ClipboardItemErrors.PayloadHashRequired);
        }

        ClipboardItem item = new(
            id,
            sourceDeviceId,
            platform,
            contentType,
            cipherText,
            nonce,
            tag,
            payloadHash,
            createdAtUtc,
            isPinned: false,
            isDeleted: false);

        return Result<ClipboardItem>.Success(item);
    }

    /// <summary>
    /// 저장소에서 읽은 값으로 항목을 재구성합니다. 규칙이 바뀐 뒤에도 과거 행을 읽을 수 있도록 검증을 다시 실행하지 않습니다.
    /// </summary>
    internal static ClipboardItem Rehydrate(
        Guid id,
        Guid sourceDeviceId,
        DevicePlatform platform,
        ContentType contentType,
        byte[] cipherText,
        byte[] nonce,
        byte[] tag,
        string payloadHash,
        DateTimeOffset createdAtUtc,
        bool isPinned,
        bool isDeleted)
    {
        ClipboardItem item = new(
            id,
            sourceDeviceId,
            platform,
            contentType,
            cipherText,
            nonce,
            tag,
            payloadHash,
            createdAtUtc,
            isPinned,
            isDeleted);

        return item;
    }

    /// <summary>
    /// 항목을 즐겨찾기에 고정합니다. 삭제된 항목은 고정할 수 없습니다.
    /// </summary>
    /// <returns>고정에 성공하면 성공 결과, 삭제된 항목이면 실패 결과</returns>
    public Result Pin()
    {
        if (IsDeleted)
        {
            return Result.Failure(ClipboardItemErrors.DeletedItemCannotBePinned);
        }

        IsPinned = true;
        return Result.Success();
    }

    /// <summary>
    /// 항목의 즐겨찾기 고정을 해제합니다.
    /// </summary>
    public void Unpin()
    {
        IsPinned = false;
    }

    /// <summary>
    /// 항목을 소프트 삭제합니다. 삭제하면 고정도 함께 해제해 삭제된 항목은 고정되지 않는다는 불변식을 지킵니다.
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        IsPinned = false;
    }
}
