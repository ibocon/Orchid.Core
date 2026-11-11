using System.Collections.Concurrent;
using Orchid.Contract.Payload;

namespace Orchid.Core.Sync;

/// <summary>
/// 디바이스별 동기화 커서 위치 및 오프라인/보류 중인 페이로드 큐를 인메모리 관리하는 저장소입니다.
/// </summary>
public sealed class SyncStateStore
{
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _deviceCursors = new();
    private readonly ConcurrentQueue<EncryptedPayload> _pendingQueue = new();

    /// <summary>
    /// 특정 디바이스의 마지막 동기화 커서 일시를 조회합니다.
    /// </summary>
    /// <param name="deviceId">디바이스 식별자</param>
    /// <returns>마지막 동기화 시점(UTC) 또는 null</returns>
    public DateTimeOffset? GetCursor(Guid deviceId)
    {
        return _deviceCursors.TryGetValue(deviceId, out DateTimeOffset timestamp) ? timestamp : null;
    }

    /// <summary>
    /// 특정 디바이스의 마지막 동기화 커서를 갱신합니다.
    /// </summary>
    /// <param name="deviceId">디바이스 식별자</param>
    /// <param name="timestamp">동기화 완료 시점(UTC)</param>
    public void UpdateCursor(Guid deviceId, DateTimeOffset timestamp)
    {
        _deviceCursors.AddOrUpdate(deviceId, timestamp, (_, _) => timestamp);
    }

    /// <summary>
    /// 네트워크 지연 또는 오프라인 상태에서 전송 보류된 페이로드를 큐에 추가합니다.
    /// </summary>
    /// <param name="payload">암호화 페이로드</param>
    public void EnqueuePending(EncryptedPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        _pendingQueue.Enqueue(payload);
    }

    /// <summary>
    /// 보류 큐에서 대기 중인 페이로드를 꺼냅니다.
    /// </summary>
    /// <param name="payload">반환된 암호화 페이로드</param>
    /// <returns>성공 여부</returns>
    public bool TryDequeuePending(out EncryptedPayload? payload)
    {
        return _pendingQueue.TryDequeue(out payload);
    }

    /// <summary>
    /// 보류 큐에 남아있는 페이로드 개수를 가져옵니다.
    /// </summary>
    public int PendingCount => _pendingQueue.Count;
}
