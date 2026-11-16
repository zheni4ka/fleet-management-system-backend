using business_logic.Constants;
using business_logic.Exceptions;
using business_logic.Interfaces;
using data_access.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace data_access.Repository
{
    public class SqlServerDatabaseApplicationLockService : IDatabaseApplicationLockService
    {
        private const string GetApplicationLockSql =
            "DECLARE @lockResult int; EXEC @lockResult = sp_getapplock @Resource = @lockResourceName, @LockMode = @lockMode, @LockOwner = @lockOwner, @LockTimeout = @lockTimeoutInMilliseconds; SELECT @lockResult;";

        private readonly FMS_DbContext _context;

        public SqlServerDatabaseApplicationLockService(FMS_DbContext context)
        {
            _context = context;
        }

        public async Task AcquireTransactionExclusiveLocksAsync(IReadOnlyCollection<string> lockResourceNames)
        {
            if (_context.Database.CurrentTransaction is null)
            {
                throw new InvalidOperationException("Transaction-owned application locks require an active database transaction.");
            }

            foreach (var lockResourceName in lockResourceNames.Distinct().Order(StringComparer.Ordinal))
            {
                var lockResult = await GetApplicationLockAsync(
                    lockResourceName,
                    SqlServerApplicationLockValues.TransactionLockOwner,
                    ApplicationLockConstraints.AssignmentLockTimeoutInMilliseconds);
                if (lockResult == SqlServerApplicationLockValues.LockTimeout)
                {
                    throw new ConcurrencyConflictException(string.Format(ExceptionMessages.ApplicationLockNotAcquiredTemplate, lockResourceName));
                }

                EnsureApplicationLockGranted(lockResourceName, lockResult);
            }
        }

        public async Task<IAsyncDisposable?> TryAcquireSessionExclusiveLockWithoutWaitingAsync(string lockResourceName)
        {
            await _context.Database.OpenConnectionAsync();
            var lockResult = await GetApplicationLockAsync(
                lockResourceName,
                SqlServerApplicationLockValues.SessionLockOwner,
                SqlServerApplicationLockValues.NoWaitLockTimeoutInMilliseconds);
            if (lockResult >= 0)
            {
                return new SqlServerSessionApplicationLock(_context, lockResourceName);
            }

            await _context.Database.CloseConnectionAsync();
            if (lockResult == SqlServerApplicationLockValues.LockTimeout)
            {
                return null;
            }

            EnsureApplicationLockGranted(lockResourceName, lockResult);
            return null;
        }

        private async Task<int> GetApplicationLockAsync(string lockResourceName, string lockOwner, int lockTimeoutInMilliseconds)
        {
            var databaseConnection = _context.Database.GetDbConnection();
            await using var getApplicationLockCommand = databaseConnection.CreateCommand();
            getApplicationLockCommand.CommandText = GetApplicationLockSql;
            getApplicationLockCommand.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            AddCommandParameter(getApplicationLockCommand, "@lockResourceName", lockResourceName);
            AddCommandParameter(getApplicationLockCommand, "@lockMode", SqlServerApplicationLockValues.ExclusiveLockMode);
            AddCommandParameter(getApplicationLockCommand, "@lockOwner", lockOwner);
            AddCommandParameter(getApplicationLockCommand, "@lockTimeoutInMilliseconds", lockTimeoutInMilliseconds);

            await _context.Database.OpenConnectionAsync();
            try
            {
                return Convert.ToInt32(await getApplicationLockCommand.ExecuteScalarAsync());
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        private static void AddCommandParameter(System.Data.Common.DbCommand databaseCommand, string parameterName, object parameterValue)
        {
            var commandParameter = databaseCommand.CreateParameter();
            commandParameter.ParameterName = parameterName;
            commandParameter.Value = parameterValue;
            databaseCommand.Parameters.Add(commandParameter);
        }

        private static void EnsureApplicationLockGranted(string lockResourceName, int lockResult)
        {
            if (lockResult < 0)
            {
                throw new InvalidOperationException(string.Format(ExceptionMessages.ApplicationLockFailedTemplate, lockResourceName, lockResult));
            }
        }
    }
}
