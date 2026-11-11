using Orchid.Contract.Payload;
using Orchid.Core.Abstraction;
using Orchid.Core.Domain.Common;
using Orchid.Core.Sync.Relay;

namespace Orchid.Core.Sync;

/// <summary>
/// 로컬 클립보드 변경 송신과 원격 페이로드 수신 반영을 연결하는 코디네이터입니다.
/// </summary>
public sealed class ClipboardSyncCoordinator : IAsyncDisposable
{
    private readonly ISyncEngine _syncEngine;
    private readonly ISystemClipboard _clipboard;
    private readonly IClipboardChangeSource _changeSource;
    private readonly IClipboardRelayClient _relay;
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private PairingKey? _pairingKey;
    private Guid _deviceId;

    /// <summary>
    /// <see cref="ClipboardSyncCoordinator"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="syncEngine">동기화 엔진</param>
    /// <param name="clipboard">시스템 클립보드</param>
    /// <param name="changeSource">로컬 클립보드 변경 알림</param>
    /// <param name="relay">릴레이 전송 클라이언트</param>
    public ClipboardSyncCoordinator(
        ISyncEngine syncEngine,
        ISystemClipboard clipboard,
        IClipboardChangeSource changeSource,
        IClipboardRelayClient relay)
    {
        _syncEngine = syncEngine ?? throw new ArgumentNullException(nameof(syncEngine));
        _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
        _changeSource = changeSource ?? throw new ArgumentNullException(nameof(changeSource));
        _relay = relay ?? throw new ArgumentNullException(nameof(relay));
    }

    /// <summary>
    /// 페어링 키로 허브에 연결하고 양방향 동기화를 시작합니다.
    /// </summary>
    /// <param name="pairingKey">페어링 키</param>
    /// <param name="deviceId">이 디바이스의 식별자</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>비동기 작업</returns>
    public async Task StartAsync(PairingKey pairingKey, Guid deviceId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pairingKey);
        _pairingKey = pairingKey;
        _deviceId = deviceId;

        _relay.PayloadReceived += OnRemoteReceivedAsync;
        _changeSource.Changed += OnLocalChangedAsync;

        // 첫 await 전에 호출해 호출자(UI) 스레드에서 변경 감시를 시작한다. 창 서브클래싱은 창을 만든 스레드에서만 성공한다.
        _changeSource.Start();

        try
        {
            await
                _relay.ConnectAsync(pairingKey.PairingId, cancellationToken)
                .ConfigureAwait(false);
            _ready.SetResult();
        }
        catch (Exception exception)
        {
            _ready.TrySetException(exception);
            throw;
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _relay.PayloadReceived -= OnRemoteReceivedAsync;
        _changeSource.Changed -= OnLocalChangedAsync;
        return _relay.DisposeAsync();
    }

    private async Task OnLocalChangedAsync()
    {
        // 연결이 끝나기 전에 발생한 변경은 연결 완료까지 기다린다.
        await
            _ready.Task
            .ConfigureAwait(false);

        PairingKey? pairingKey = _pairingKey;
        string? text = await
            _clipboard.GetTextAsync()
            .ConfigureAwait(false);
        if (string.IsNullOrEmpty(text) || pairingKey is null)
        {
            return;
        }

        Result<EncryptedPayload> result = await
            _syncEngine.ProcessLocalClipboardAsync(text, _deviceId, pairingKey.GetKeyBytes())
            .ConfigureAwait(false);

        // 중복(원격 수신 직후의 로컬 변경 이벤트 포함)만 흡수하고 그 외 실패는 호출자에게 드러낸다.
        if (result.IsFailure && result.Error == SyncEngine.DuplicateContent)
        {
            return;
        }

        await
            _relay.SendAsync(pairingKey.PairingId, Unwrap(result))
            .ConfigureAwait(false);
    }

    private async Task OnRemoteReceivedAsync(EncryptedPayload payload)
    {
        PairingKey? pairingKey = _pairingKey;
        if (pairingKey is null)
        {
            return;
        }

        Result<string> result = await
            _syncEngine.ProcessRemotePayloadAsync(payload, pairingKey.GetKeyBytes())
            .ConfigureAwait(false);
        await
            _clipboard.SetTextAsync(Unwrap(result))
            .ConfigureAwait(false);
    }

    private static T Unwrap<T>(Result<T> result)
    {
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"{result.Error.Code}: {result.Error.Message}");
    }
}
