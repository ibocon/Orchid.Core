using SQLite;

namespace Orchid.Core.Storage;

/// <summary>
/// 로컬 SQLite 데이터베이스 스키마 마이그레이션 및 버전 관리를 수행합니다.
/// </summary>
public sealed class DatabaseMigrator
{
    private readonly SQLiteAsyncConnection _db;

    /// <summary>
    /// <see cref="DatabaseMigrator"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="db">SQLite 연결 인스턴스</param>
    public DatabaseMigrator(SQLiteAsyncConnection db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <summary>
    /// 보류 중인 스키마 마이그레이션을 비동기적으로 적용합니다.
    /// </summary>
    public async Task MigrateAsync()
    {
        // SQLite-net의 CreateTableAsync는 테이블이 이미 존재하면 새 컬럼을 자동 추가하는 멱등성(Idempotent) 마이그레이션을 수행합니다.
        await _db.CreateTableAsync<Domain.Entity.ClipboardItem>().ConfigureAwait(false);
        await _db.CreateTableAsync<Domain.Entity.DeviceSession>().ConfigureAwait(false);
    }
}
