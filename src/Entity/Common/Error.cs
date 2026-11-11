using System.Diagnostics.CodeAnalysis;

namespace Orchid.Core.Domain.Common;

/// <summary>
/// 도메인 및 작업 수행 중 발생하는 오류를 나타냅니다.
/// </summary>
/// <param name="Code">오류 고유 코드</param>
/// <param name="Message">오류 상세 메시지</param>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Domain error pattern")]
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// 오류가 없음을 나타내는 빈 오류 인스턴스입니다.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// 지정되지 않은 일반 오류를 생성합니다.
    /// </summary>
    /// <param name="message">오류 메시지</param>
    /// <returns>생성된 <see cref="Error"/> 인스턴스</returns>
    public static Error General(string message)
    {
        return new("General.Error", message);
    }

    /// <summary>
    /// 유효성 검증 실패 오류를 생성합니다.
    /// </summary>
    /// <param name="message">검증 실패 메시지</param>
    /// <returns>생성된 <see cref="Error"/> 인스턴스</returns>
    public static Error Validation(string message)
    {
        return new("Validation.Failed", message);
    }

    /// <summary>
    /// 엔티티 또는 리소스 미발견 오류를 생성합니다.
    /// </summary>
    /// <param name="resourceName">대상 리소스 이름</param>
    /// <param name="key">대상 식별 키</param>
    /// <returns>생성된 <see cref="Error"/> 인스턴스</returns>
    public static Error NotFound(string resourceName, object key)
    {
        return new("Resource.NotFound", $"{resourceName} with key '{key}' was not found.");
    }
}
