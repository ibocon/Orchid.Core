namespace Orchid.Core.Abstraction;

/// <summary>
/// 모바일 백그라운드 수명주기 및 주기적 동기화 워커 계약입니다.
/// </summary>
public interface IMobileSyncBackgroundWorker
{
    /// <summary>
    /// 백그라운드 워커 활성화 여부
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// 백그라운드 동기화 작업을 시작합니다.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 백그라운드 동기화 작업을 중지합니다.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
