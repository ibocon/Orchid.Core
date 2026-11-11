using FluentAssertions;
using Moq;
using Orchid.Core.Cryptography;
using Orchid.Core.Storage;
using Orchid.Core.Sync;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="SyncEngine"/>의 중복 해시 감지 및 E2EE 클립보드 처리 흐름을 검증하는 단위 테스트입니다.
/// </summary>
public sealed class SyncEngineTests
{
    [Fact]
    public async Task ProcessLocalClipboardAsync_When_Duplicate_Content_Should_Return_Failure()
    {
        // Given
        var cryptoMock = new Mock<IAesGcmCryptoService>();
        var repoMock = new Mock<IClipboardRepository>();

        repoMock.Setup(r => r.ExistsByHashAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var engine = new SyncEngine(cryptoMock.Object, repoMock.Object);
        byte[] key = new byte[32];

        // When
        var result = await engine.ProcessLocalClipboardAsync("Duplicate Text", Guid.NewGuid(), key);

        // Then
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Duplicate");
    }
}
