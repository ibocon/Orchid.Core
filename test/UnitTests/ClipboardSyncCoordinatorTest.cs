using System.Text;
using FluentAssertions;
using Orchid.Contract.Payload;
using Orchid.Core.Cryptography;
using Orchid.Core.Sync;
using Orchid.Core.Sync.Relay;
using Orchid.UnitTests.Core.Fixture;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="ClipboardSyncCoordinator"/>의 송신·수신 반영·되송신 억제를 검증하는 단위 테스트입니다.
/// </summary>
public sealed class ClipboardSyncCoordinatorTest
{
    [Fact]
    public async Task OnLocalChangeWhenTextCopiedShouldSendEncryptedPayloadToPairingGroup()
    {
        // Given
        PairingKey key = PairingKey.Generate();
        (ClipboardSyncCoordinator sut, FakeRelayClient relay, FakeClipboard clipboard, FakeChangeSource source) = CreateSut();
        await sut.StartAsync(key, Guid.NewGuid());
        clipboard.Text = "hello orchid";

        // When
        await source.RaiseChangedAsync();

        // Then
        relay.Sent.Should().ContainSingle().Which.PairingId.Should().Be(key.PairingId);
    }

    [Fact]
    public async Task OnRemoteWhenPayloadReceivedShouldWritePlainTextToClipboard()
    {
        // Given
        PairingKey key = PairingKey.Generate();
        (ClipboardSyncCoordinator sut, FakeRelayClient relay, FakeClipboard clipboard, _) = CreateSut();
        await sut.StartAsync(key, Guid.NewGuid());

        // When
        await relay.RaiseReceivedAsync(Encrypt(key, "from windows"));

        // Then
        clipboard.Text.Should().Be("from windows");
    }

    [Fact]
    public async Task OnLocalChangeWhenRemotePayloadJustAppliedShouldNotEchoBack()
    {
        // Given
        PairingKey key = PairingKey.Generate();
        (ClipboardSyncCoordinator sut, FakeRelayClient relay, _, FakeChangeSource source) = CreateSut();
        await sut.StartAsync(key, Guid.NewGuid());
        await relay.RaiseReceivedAsync(Encrypt(key, "from windows"));

        // When
        await source.RaiseChangedAsync();

        // Then
        relay.Sent.Should().BeEmpty();
    }

    [Fact]
    public async Task OnRemoteWhenPayloadEncryptedWithOtherKeyShouldThrowAndKeepClipboard()
    {
        // Given
        PairingKey key = PairingKey.Generate();
        (ClipboardSyncCoordinator sut, FakeRelayClient relay, FakeClipboard clipboard, _) = CreateSut();
        await sut.StartAsync(key, Guid.NewGuid());
        EncryptedPayload foreign = Encrypt(PairingKey.Generate(), "intruder");

        // When
        Func<Task> act = () => relay.RaiseReceivedAsync(foreign);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>();
        clipboard.Text.Should().BeNull();
    }

    [Fact]
    public async Task StartAsyncWhenConnectPendingShouldStartChangeSourceOnCallerThreadBeforeConnectCompletes()
    {
        // Given
        PairingKey key = PairingKey.Generate();
        (ClipboardSyncCoordinator sut, FakeRelayClient relay, _, FakeChangeSource source) = CreateSut(completeConnect: false);

        // When
        Task start = sut.StartAsync(key, Guid.NewGuid());

        // Then
        start.IsCompleted.Should().BeFalse();
        source.StartedOnThreadId.Should().Be(Environment.CurrentManagedThreadId);

        relay.ConnectGate.SetResult();
        await start;
    }

    [Fact]
    public async Task OnLocalChangeWhenRaisedBeforeConnectCompletesShouldSendAfterConnect()
    {
        // Given
        PairingKey key = PairingKey.Generate();
        (ClipboardSyncCoordinator sut, FakeRelayClient relay, FakeClipboard clipboard, FakeChangeSource source) = CreateSut(completeConnect: false);
        Task start = sut.StartAsync(key, Guid.NewGuid());
        clipboard.Text = "early copy";
        Task changed = source.RaiseChangedAsync();

        // When
        relay.ConnectGate.SetResult();
        await start;
        await changed;

        // Then
        relay.Sent.Should().ContainSingle();
    }

    private static (ClipboardSyncCoordinator Sut, FakeRelayClient Relay, FakeClipboard Clipboard, FakeChangeSource Source) CreateSut(bool completeConnect = true)
    {
        FakeRelayClient relay = new();
        if (completeConnect)
        {
            relay.ConnectGate.SetResult();
        }

        FakeClipboard clipboard = new();
        FakeChangeSource source = new();
        SyncEngine engine = new(new AesGcmCryptoService(), InMemoryRepository.Create());
        return (new ClipboardSyncCoordinator(engine, clipboard, source, relay), relay, clipboard, source);
    }

    private static EncryptedPayload Encrypt(PairingKey key, string text)
    {
        AesGcmCryptoService crypto = new();
        (byte[] cipherText, byte[] nonce, byte[] tag) = crypto.Encrypt(Encoding.UTF8.GetBytes(text), key.GetKeyBytes()).Value;
        return new EncryptedPayload(Guid.NewGuid(), cipherText, nonce, tag, "AES-256-GCM", DateTimeOffset.UtcNow);
    }
}
