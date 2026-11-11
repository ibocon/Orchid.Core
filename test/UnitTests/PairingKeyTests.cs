using FluentAssertions;
using Orchid.Core.Domain.Common;
using Orchid.Core.Sync.Relay;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="PairingKey"/>의 직렬화 왕복과 형식 검증을 확인하는 단위 테스트입니다.
/// </summary>
public sealed class PairingKeyTests
{
    [Fact]
    public void Parse_When_Payload_Roundtrips_Should_Keep_PairingId_And_Key()
    {
        // Given
        PairingKey original = PairingKey.Generate();

        // When
        Result<PairingKey> parsed = PairingKey.Parse(original.ToPayload());

        // Then
        parsed.IsSuccess.Should().BeTrue();
        parsed.Value.PairingId.Should().Be(original.PairingId);
        parsed.Value.GetKeyBytes().Should().Equal(original.GetKeyBytes());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("other-prefix:AAAA")]
    [InlineData("orchid-pair:v1:AAAA")]
    [InlineData("orchid-pair:v1:@@@@")]
    public void Parse_When_Payload_Invalid_Should_Return_Validation_Failure(string? payload)
    {
        // When
        Result<PairingKey> parsed = PairingKey.Parse(payload);

        // Then
        parsed.IsFailure.Should().BeTrue();
        parsed.Error.Code.Should().Be("Validation.Failed");
    }

    [Fact]
    public void GetKeyBytes_When_Mutated_Should_Not_Affect_Key()
    {
        // Given
        PairingKey key = PairingKey.Generate();
        byte[] copy = key.GetKeyBytes();

        // When
        copy[0] ^= 0xFF;

        // Then
        key.GetKeyBytes()[0].Should().NotBe(copy[0]);
    }
}
