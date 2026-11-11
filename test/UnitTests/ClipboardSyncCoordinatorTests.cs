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
public sealed class ClipboardSyncCoordinatorTests
{
    [Fact]
    public async Task OnLocalChange_When_Text_Copied_Should_Send_Encrypted_Payload_To_Pairing_Group()
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
    public async Task OnRemote_When_Payload_Received_Should_Write_Plain_Text_To_Clipboard()
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
    public async Task OnLocalChange_When_Remote_Payload_Just_Applied_Should_Not_Echo_Back()
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
    public async Task OnRemote_When_Payload_Encrypted_With_Other_Key_Should_Throw_And_Keep_Clipboard()
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
    public async Task StartAsync_When_Connect_Pending_Should_Start_Change_Source_On_Caller_Thread_Before_Connect_Completes()
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
    public async Task OnLocalChange_When_Raised_Before_Connect_Completes_Should_Send_After_Connect()
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
