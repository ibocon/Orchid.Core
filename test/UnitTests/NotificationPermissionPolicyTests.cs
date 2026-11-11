using FluentAssertions;
using Orchid.Core.Notification;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="NotificationPermissionPolicy"/>의 요청 여부 판정을 확인하는 단위 테스트입니다.
/// </summary>
public sealed class NotificationPermissionPolicyTests
{
    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    public void ShouldRequest_When_State_Given_Should_Request_Only_When_Neither_Granted_Nor_Requested(bool granted, bool requestedBefore, bool expected)
    {
        // Given
        // When
        bool actual = NotificationPermissionPolicy.ShouldRequest(granted, requestedBefore);

        // Then
        actual.Should().Be(expected);
    }
}
