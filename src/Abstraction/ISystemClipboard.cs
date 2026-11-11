namespace Orchid.Core.Abstraction;

/// <summary>
/// 각 OS 플랫폼의 네이티브 시스템 클립보드 읽기/쓰기를 추상화하는 계약 인터페이스입니다.
/// </summary>
public interface ISystemClipboard
{
    /// <summary>
    /// 시스템 클립보드로부터 평문 텍스트를 비동기로 읽어옵니다.
    /// </summary>
    /// <returns>클립보드 텍스트 또는 null</returns>
    Task<string?> GetTextAsync();

    /// <summary>
    /// 시스템 클립보드에 평문 텍스트를 비동기로 설정합니다.
    /// </summary>
    /// <param name="text">설정할 텍스트</param>
    Task SetTextAsync(string text);
}
