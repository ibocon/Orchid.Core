namespace Orchid.Core.Abstraction;

/// <summary>
/// 로컬 변경을 송신하지 않는 수신 전용 플랫폼용 구현체입니다.
/// </summary>
public sealed class NullClipboardChangeSource : IClipboardChangeSource
{
    /// <inheritdoc />
    public event Func<Task>? Changed
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public void Start()
    {
    }
}
