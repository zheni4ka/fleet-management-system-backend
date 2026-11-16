using business_logic.Interfaces;

namespace business_logic_tests.Infrastructure
{
    public sealed class TestDatabaseApplicationLockService : IDatabaseApplicationLockService
    {
        public Task AcquireTransactionExclusiveLocksAsync(IReadOnlyCollection<string> lockResourceNames)
        {
            return Task.CompletedTask;
        }

        public Task<IAsyncDisposable?> TryAcquireSessionExclusiveLockWithoutWaitingAsync(string lockResourceName)
        {
            return Task.FromResult<IAsyncDisposable?>(new TestSessionApplicationLock());
        }
    }
}
