using System.Diagnostics.CodeAnalysis;
using Orchid.Contract.Model;
using SQLite;

namespace Orchid.Core.Domain.Entity;

/// <summary>
/// 로컬 저장소 및 도메인 로직에서 취급하는 클립보드 항목 엔티티입니다.
/// </summary>
[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Cryptographic binary payload required for SQLite storage")]
public sealed class ClipboardItem
{
    /// <summary>
    /// <see cref="ClipboardItem"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public ClipboardItem(
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
        Id = id;
        SourceDeviceId = sourceDeviceId;
        Platform = platform;
        ContentType = contentType;
        CipherText = cipherText;
        Nonce = nonce;
        Tag = tag;
        PayloadHash = payloadHash;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// 기본 생성자 (SQLite ORM 역직렬화용)
    /// </summary>
    public ClipboardItem()
    {
        CipherText = [];
        Nonce = [];
        Tag = [];
        PayloadHash = string.Empty;
    }

    /// <summary>
    /// 항목 고유 식별자
    /// </summary>
    [PrimaryKey]
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 발신 디바이스 식별자
    /// </summary>
    public Guid SourceDeviceId { get; init; }

    /// <summary>
    /// 발신 플랫폼
    /// </summary>
    public DevicePlatform Platform { get; init; }

    /// <summary>
    /// 콘텐츠 형식
    /// </summary>
    public ContentType ContentType { get; init; }

    /// <summary>
    /// 암호화된 데이터 바이트 배열
    /// </summary>
    public byte[] CipherText { get; init; }

    /// <summary>
    /// 암호화에 사용된 논스(12바이트)
    /// </summary>
    public byte[] Nonce { get; init; }

    /// <summary>
    /// 인증 태그(16바이트)
    /// </summary>
    public byte[] Tag { get; init; }

    /// <summary>
    /// 중복 방지용 해시
    /// </summary>
    [Indexed]
    public string PayloadHash { get; init; }

    /// <summary>
    /// 즐겨찾기 고정 여부
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// 삭제 여부 (소프트 삭제)
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 항목 생성 일시 (UTC)
    /// </summary>
    [Indexed]
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
