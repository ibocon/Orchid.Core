using FluentAssertions;
using Moq;
using Orchid.Core.Cryptography;
using Orchid.Core.Domain.Common;
using Orchid.Core.Storage;
using Orchid.Core.Sync;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="SyncEngine"/>의 중복 해시 감지 및 E2EE 클립보드 처리 흐름을 검증하는 단위 테스트입니다.
/// </summary>
public sealed class SyncEngineTest
{
    [Fact]
    public async Task ProcessLocalClipboardAsyncWhenDuplicateContentShouldReturnFailure()
    {
        // Given
        Mock<IAesGcmCryptoService> cryptoMock = new Mock<IAesGcmCryptoService>();
        Mock<IClipboardRepository> repoMock = new Mock<IClipboardRepository>();

        repoMock.Setup(r => r.ExistsByHashAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        SyncEngine engine = new SyncEngine(cryptoMock.Object, repoMock.Object);
        byte[] key = new byte[32];

        // When
        Result<EncryptedPayload> result = await engine.ProcessLocalClipboardAsync("Duplicate Text", Guid.NewGuid(), key);

        // Then
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Duplicate");
    }
}
