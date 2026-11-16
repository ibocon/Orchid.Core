using FluentAssertions;
using Orchid.Contract.Model;
using Orchid.Core.Domain.Entity;
using Orchid.Core.Storage;
using Orchid.UnitTests.Core.Fixture;
using SQLite;
using Xunit;

namespace Orchid.UnitTests.Core;

/// <summary>
/// <see cref="SqliteClipboardRepository"/>의 Row 변환 왕복과 기존 DB 호환을 검증하는 단위 테스트입니다.
/// </summary>
public sealed class SqliteClipboardRepositoryTest : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"orchid-core-repo-{Guid.NewGuid():N}");

    public SqliteClipboardRepositoryTest()
    {
        Directory.CreateDirectory(_directory);
    }

    private string DatabasePath => Path.Combine(_directory, "orchid.db");

    public void Dispose()
    {
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
            // temp 폴더 정리는 best-effort
        }
    }

    [Fact]
    public async Task UpsertThenGetByIdShouldPreserveEveryFieldIncludingPinnedState()
    {
        // Given
        SqliteClipboardRepository sut = new(DatabasePath);
        ClipboardItem item = ClipboardItemFixture.CreateValid();
        item.Pin();

        // When
        await sut.UpsertAsync(item);
        ClipboardItem? loaded = await sut.GetByIdAsync(item.Id);

        // Then
        loaded.Should().NotBeNull();
        loaded.Id.Should().Be(item.Id);
        loaded.PayloadHash.Should().Be(item.PayloadHash);
        loaded.CipherText.Should().Equal(item.CipherText);
        loaded.IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWhenItemIsPinnedShouldHideItemFromRecent()
    {
        // Given
        SqliteClipboardRepository sut = new(DatabasePath);
        ClipboardItem item = ClipboardItemFixture.CreateValid();
        item.Pin();
        await sut.UpsertAsync(item);

        // When
        await sut.DeleteAsync(item.Id);
        IReadOnlyList<ClipboardItem> recent = await sut.GetRecentAsync();

        // Then
        recent.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdWhenRowWasWrittenWithLegacyShapeShouldLoadWithoutRevalidation()
    {
        // Given
        // ✅ 기존 스키마와 같은 컬럼을 가진 행을 직접 기록한다. 논스 길이가 규칙(12바이트)에 맞지 않아도 읽혀야 한다
        Guid id = Guid.NewGuid();
        SQLiteAsyncConnection legacy = new(DatabasePath);
        await legacy.CreateTableAsync<LegacyClipboardItemRow>();
        await legacy.InsertAsync(new LegacyClipboardItemRow
        {
            Id = id,
            Platform = DevicePlatform.Windows,
            ContentType = ContentType.Text,
            CipherText = [1],
            Nonce = [2, 3, 4],
            Tag = [5],
            PayloadHash = "legacy-hash",
            IsPinned = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        });
        await legacy.CloseAsync();
        SqliteClipboardRepository sut = new(DatabasePath);

        // When
        ClipboardItem? loaded = await sut.GetByIdAsync(id);

        // Then
        loaded.Should().NotBeNull();
        loaded.PayloadHash.Should().Be("legacy-hash");
        loaded.IsPinned.Should().BeTrue();
    }

    [Table("ClipboardItem")]
    private sealed class LegacyClipboardItemRow
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public Guid SourceDeviceId { get; set; }

        public DevicePlatform Platform { get; set; }

        public ContentType ContentType { get; set; }

        public byte[] CipherText { get; set; } = [];

        public byte[] Nonce { get; set; } = [];

        public byte[] Tag { get; set; } = [];

        [Indexed]
        public string PayloadHash { get; set; } = string.Empty;

        public bool IsPinned { get; set; }

        public bool IsDeleted { get; set; }

        [Indexed]
        public DateTimeOffset CreatedAtUtc { get; set; }
    }
}
