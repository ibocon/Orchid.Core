using FluentAssertions;
using Orchid.Contract.Model;
using Orchid.Core.Domain.Common;
using Orchid.Core.Domain.Entity;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="DeviceSession"/>의 생성 불변식과 상태 전이를 검증하는 단위 테스트입니다.
/// </summary>
public sealed class DeviceSessionTest
{
    private static readonly DateTimeOffset PairedAt = new(2026, 11, 16, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateWhenInputsAreValidShouldStartTrustedAndSeenAtPairingTime()
    {
        // Given & When
        Result<DeviceSession> result = DeviceSession.Create(Guid.NewGuid(), "MacBook Pro", DevicePlatform.MacCatalyst, [1, 2, 3], PairedAt);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.IsTrusted.Should().BeTrue();
        result.Value.LastSeenAtUtc.Should().Be(PairedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void CreateWhenDeviceNameIsBlankShouldFail(string name)
    {
        // Given & When
        Result<DeviceSession> result = DeviceSession.Create(Guid.NewGuid(), name, DevicePlatform.MacCatalyst, [1], PairedAt);

        // Then
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CreateWhenPublicKeyIsEmptyShouldFail()
    {
        // Given & When
        Result<DeviceSession> result = DeviceSession.Create(Guid.NewGuid(), "MacBook Pro", DevicePlatform.MacCatalyst, [], PairedAt);

        // Then
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void MarkSeenWhenEarlierThanLastSeenShouldKeepLastSeen()
    {
        // Given
        DeviceSession session = DeviceSession.Create(Guid.NewGuid(), "Galaxy S24", DevicePlatform.Android, [1], PairedAt).Value;

        // When
        session.MarkSeen(PairedAt.AddMinutes(-5));

        // Then
        session.LastSeenAtUtc.Should().Be(PairedAt);
    }

    [Fact]
    public void RenameWhenBlankShouldFailAndKeepName()
    {
        // Given
        DeviceSession session = DeviceSession.Create(Guid.NewGuid(), "Galaxy S24", DevicePlatform.Android, [1], PairedAt).Value;

        // When
        Result result = session.Rename(" ");

        // Then
        result.IsFailure.Should().BeTrue();
        session.DeviceName.Should().Be("Galaxy S24");
    }

    [Fact]
    public void RevokeThenTrustShouldToggleIsTrusted()
    {
        // Given
        DeviceSession session = DeviceSession.Create(Guid.NewGuid(), "Galaxy S24", DevicePlatform.Android, [1], PairedAt).Value;

        // When
        session.Revoke();
        bool trustedAfterRevoke = session.IsTrusted;
        session.Trust();

        // Then
        trustedAfterRevoke.Should().BeFalse();
        session.IsTrusted.Should().BeTrue();
    }
}
