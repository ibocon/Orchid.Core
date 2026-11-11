using Orchid.Core.Domain.Entity;
using SQLite;

namespace Orchid.Core.Storage;

/// <summary>
/// SQLite-net 기반의 크로스 플랫폼 경량 로컬 클립보드 저장소 구현체입니다.
/// </summary>
public sealed class SqliteClipboardRepository : IClipboardRepository
{
    private readonly SQLiteAsyncConnection _db;
    private bool _initialized;

    /// <summary>
    /// <see cref="SqliteClipboardRepository"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="databasePath">SQLite 데이터베이스 파일 경로</param>
    public SqliteClipboardRepository(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _db = new SQLiteAsyncConnection(databasePath);
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _db.CreateTableAsync<ClipboardItem>().ConfigureAwait(false);
        await _db.CreateTableAsync<DeviceSession>().ConfigureAwait(false);
        _initialized = true;
    }

    /// <inheritdoc />
    public async Task UpsertAsync(ClipboardItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        await InitializeAsync().ConfigureAwait(false);
        await _db.InsertOrReplaceAsync(item).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ClipboardItem?> GetByIdAsync(Guid id)
    {
        await InitializeAsync().ConfigureAwait(false);
        return await _db.Table<ClipboardItem>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ClipboardItem>> GetRecentAsync(int skip = 0, int take = 50)
    {
        await InitializeAsync().ConfigureAwait(false);
        List<ClipboardItem> items = await _db.Table<ClipboardItem>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync()
            .ConfigureAwait(false);

        return items;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByHashAsync(string payloadHash)
    {
        if (string.IsNullOrWhiteSpace(payloadHash))
        {
            return false;
        }

        await InitializeAsync().ConfigureAwait(false);
        int count = await _db.Table<ClipboardItem>()
            .Where(x => x.PayloadHash == payloadHash && !x.IsDeleted)
            .CountAsync()
            .ConfigureAwait(false);

        return count > 0;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        await InitializeAsync().ConfigureAwait(false);
        ClipboardItem? item = await GetByIdAsync(id).ConfigureAwait(false);
        if (item is not null)
        {
            item.IsDeleted = true;
            await _db.UpdateAsync(item).ConfigureAwait(false);
        }
    }
}
