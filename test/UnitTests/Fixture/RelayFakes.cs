using Moq;
using Orchid.Contract.Payload;
using Orchid.Core.Abstraction;
using Orchid.Core.Domain.Entity;
using Orchid.Core.Storage;
using Orchid.Core.Sync.Relay;

namespace Orchid.UnitTests.Core.Fixture;

internal sealed class FakeRelayClient : IClipboardRelayClient
{
    public List<(string PairingId, EncryptedPayload Payload)> Sent { get; } = [];

    public TaskCompletionSource ConnectGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public event Func<EncryptedPayload, Task>? PayloadReceived;

    public Task ConnectAsync(string pairingId, CancellationToken cancellationToken = default)
    {
        return ConnectGate.Task;
    }

    public Task SendAsync(string pairingId, EncryptedPayload payload, CancellationToken cancellationToken = default)
    {
        Sent.Add((pairingId, payload));
        return Task.CompletedTask;
    }

    public Task RaiseReceivedAsync(EncryptedPayload payload)
    {
        return PayloadReceived!.Invoke(payload);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

internal sealed class FakeClipboard : ISystemClipboard
{
    public string? Text { get; set; }

    public Task<string?> GetTextAsync()
    {
        return Task.FromResult(Text);
    }

    public Task SetTextAsync(string text)
    {
        Text = text;
        return Task.CompletedTask;
    }
}

internal sealed class FakeChangeSource : IClipboardChangeSource
{
    public int? StartedOnThreadId { get; private set; }

    public event Func<Task>? Changed;

    public void Start()
    {
        StartedOnThreadId = Environment.CurrentManagedThreadId;
    }

    public Task RaiseChangedAsync()
    {
        return Changed!.Invoke();
    }
}

internal static class InMemoryRepository
{
    public static IClipboardRepository Create()
    {
        HashSet<string> hashes = [];
        Mock<IClipboardRepository> mock = new();
        mock.Setup(r => r.ExistsByHashAsync(It.IsAny<string>()))
            .ReturnsAsync((string hash) => hashes.Contains(hash));
        mock.Setup(r => r.UpsertAsync(It.IsAny<ClipboardItem>()))
            .Callback((ClipboardItem item) => hashes.Add(item.PayloadHash))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }
}
