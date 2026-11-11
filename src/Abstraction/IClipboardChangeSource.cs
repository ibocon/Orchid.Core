namespace Orchid.Core.Abstraction;

/// <summary>
/// 로컬 시스템 클립보드 변경을 알리는 계약입니다.
/// </summary>
public interface IClipboardChangeSource
{
    /// <summary>
    /// 클립보드 내용이 바뀌면 발생합니다.
    /// </summary>
    event Func<Task>? Changed;

    /// <summary>
    /// 변경 감시를 시작합니다.
    /// </summary>
    void Start();
}
