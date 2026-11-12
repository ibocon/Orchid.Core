using FluentAssertions;
using Orchid.Core.Domain.Common;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="Result"/> 및 <see cref="Error"/>의 불변식과 예외 안전성을 검증하는 단위 테스트입니다.
/// </summary>
public sealed class ResultAndErrorTest
{
    [Fact]
    public void ResultSuccessShouldHaveIsSuccessTrueAndNoError()
    {
        // Given & When
        Result result = Result.Success();

        // Then
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void ResultOfTFailureShouldThrowWhenAccessingValue()
    {
        // Given
        Result<string> result = Result<string>.Failure(Error.General("Failed operation"));

        // When & Then
        Action act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Failed operation*");
    }
}
