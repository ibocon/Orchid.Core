using FluentAssertions;
using Orchid.Core.Domain.Common;
using Orchid.Core.Domain.Entity;
using Orchid.UnitTests.Core.Fixture;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="ClipboardItem"/>의 생성 불변식과 상태 전이를 검증하는 단위 테스트입니다.
/// </summary>
public sealed class ClipboardItemTest
{
    [Fact]
    public void CreateWhenInputsAreValidShouldReturnUnpinnedUndeletedItem()
    {
        // Given & When
        Result<ClipboardItem> result = ClipboardItemFixture.Create();

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.IsPinned.Should().BeFalse();
        result.Value.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(11)]
    [InlineData(13)]
    public void CreateWhenNonceLengthIsNotTwelveShouldFailWithInvalidNonceLength(int length)
    {
        // Given & When
        Result<ClipboardItem> result = ClipboardItemFixture.Create(nonce: new byte[length]);

        // Then
        // ✅ 실패 이유가 오류 코드로 드러난다
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClipboardItemErrors.InvalidNonceLength);
    }

    [Fact]
    public void CreateWhenTagLengthIsNotSixteenShouldFailWithInvalidTagLength()
    {
        // Given & When
        Result<ClipboardItem> result = ClipboardItemFixture.Create(tag: new byte[15]);

        // Then
        result.Error.Should().Be(ClipboardItemErrors.InvalidTagLength);
    }

    [Fact]
    public void CreateWhenIdIsEmptyShouldFailWithIdRequired()
    {
        // Given & When
        Result<ClipboardItem> result = ClipboardItemFixture.Create(id: Guid.Empty);

        // Then
        result.Error.Should().Be(ClipboardItemErrors.IdRequired);
    }

    [Fact]
    public void CreateWhenCipherTextIsEmptyShouldFailWithCipherTextRequired()
    {
        // Given & When
        Result<ClipboardItem> result = ClipboardItemFixture.Create(cipherText: []);

        // Then
        result.Error.Should().Be(ClipboardItemErrors.CipherTextRequired);
    }

    [Fact]
    public void CreateWhenPayloadHashIsBlankShouldFailWithPayloadHashRequired()
    {
        // Given & When
        Result<ClipboardItem> result = ClipboardItemFixture.Create(payloadHash: " ");

        // Then
        result.Error.Should().Be(ClipboardItemErrors.PayloadHashRequired);
    }

    [Fact]
    public void PinThenUnpinShouldToggleIsPinned()
    {
        // Given
        ClipboardItem item = ClipboardItemFixture.CreateValid();

        // When
        Result pinResult = item.Pin();
        bool pinnedAfterPin = item.IsPinned;
        item.Unpin();

        // Then
        pinResult.IsSuccess.Should().BeTrue();
        pinnedAfterPin.Should().BeTrue();
        item.IsPinned.Should().BeFalse();
    }

    [Fact]
    public void DeleteWhenPinnedShouldMarkDeletedAndClearPin()
    {
        // Given
        ClipboardItem item = ClipboardItemFixture.CreateValid();
        item.Pin();

        // When
        item.Delete();

        // Then
        // ✅ 삭제된 항목은 고정되지 않는다는 불변식
        item.IsDeleted.Should().BeTrue();
        item.IsPinned.Should().BeFalse();
    }

    [Fact]
    public void PinWhenDeletedShouldFailAndKeepUnpinned()
    {
        // Given
        ClipboardItem item = ClipboardItemFixture.CreateValid();
        item.Delete();

        // When
        Result result = item.Pin();

        // Then
        result.Error.Should().Be(ClipboardItemErrors.DeletedItemCannotBePinned);
        item.IsPinned.Should().BeFalse();
    }
}
