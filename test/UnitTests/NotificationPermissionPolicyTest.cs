using FluentAssertions;
using Orchid.Core.Notification;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="NotificationPermissionPolicy"/>의 요청 여부 판정을 확인하는 단위 테스트입니다.
/// </summary>
public sealed class NotificationPermissionPolicyTest
{
    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    public void ShouldRequestWhenStateGivenShouldRequestOnlyWhenNeitherGrantedNorRequested(bool granted, bool requestedBefore, bool expected)
    {
        // Given
        // When
        bool actual = NotificationPermissionPolicy.ShouldRequest(granted, requestedBefore);

        // Then
        actual.Should().Be(expected);
    }
}
