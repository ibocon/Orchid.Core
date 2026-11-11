using Orchid.Core.Domain.Entity;

namespace Orchid.Core.Storage;

/// <summary>
/// 암호화된 로컬 클립보드 히스토리를 저장하고 조회하는 저장소 계약입니다.
/// </summary>
public interface IClipboardRepository
{
    /// <summary>
    /// 저장소를 초기화하고 필요한 테이블을 생성합니다.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 클립보드 항목을 로컬에 추가하거나 갱신합니다.
    /// </summary>
    /// <param name="item">저장할 클립보드 엔티티</param>
    Task UpsertAsync(ClipboardItem item);

    /// <summary>
    /// 특정 식별자의 클립보드 항목을 조회합니다.
    /// </summary>
    /// <param name="id">항목 고유 식별자</param>
    /// <returns>클립보드 항목 또는 null</returns>
    Task<ClipboardItem?> GetByIdAsync(Guid id);

    /// <summary>
    /// 최근 클립보드 항목 목록을 페이징하여 조회합니다.
    /// </summary>
    /// <param name="skip">건너뛸 항목 수</param>
    /// <param name="take">가져올 항목 수</param>
    /// <returns>클립보드 항목 컬렉션</returns>
    Task<IReadOnlyList<ClipboardItem>> GetRecentAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 페이로드 해시를 기준으로 중복 항목이 존재하는지 확인합니다.
    /// </summary>
    /// <param name="payloadHash">검사할 페이로드 해시</param>
    /// <returns>존재 여부</returns>
    Task<bool> ExistsByHashAsync(string payloadHash);

    /// <summary>
    /// 특정 항목을 소프트 삭제 처리합니다.
    /// </summary>
    /// <param name="id">삭제할 항목 식별자</param>
    Task DeleteAsync(Guid id);
}
