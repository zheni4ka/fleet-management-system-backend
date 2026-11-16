using data_access.data;
using Microsoft.EntityFrameworkCore;

namespace data_access.Repository
{
    public sealed class SqlServerSessionApplicationLock : IAsyncDisposable
    {
        private const string ReleaseApplicationLockSql =
            "EXEC sp_releaseapplock @Resource = {0}, @LockOwner = {1};";

        private readonly FMS_DbContext _context;
        private readonly string _lockResourceName;

        public SqlServerSessionApplicationLock(FMS_DbContext context, string lockResourceName)
        {
            _context = context;
            _lockResourceName = lockResourceName;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    ReleaseApplicationLockSql,
                    _lockResourceName,
                    SqlServerApplicationLockValues.SessionLockOwner);
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }
}
