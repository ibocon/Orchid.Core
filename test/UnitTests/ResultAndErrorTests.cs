using FluentAssertions;
using Orchid.Core.Domain.Common;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="Result"/> 및 <see cref="Error"/>의 불변식과 예외 안전성을 검증하는 단위 테스트입니다.
/// </summary>
public sealed class ResultAndErrorTests
{
    [Fact]
    public void Result_Success_Should_Have_IsSuccess_True_And_NoError()
    {
        // Given & When
        var result = Result.Success();

        // Then
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void ResultOfT_Failure_Should_Throw_When_Accessing_Value()
    {
        // Given
        var result = Result<string>.Failure(Error.General("Failed operation"));

        // When & Then
        Action act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Failed operation*");
    }
}
