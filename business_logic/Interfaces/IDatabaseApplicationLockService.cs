namespace business_logic.Interfaces
{
    public interface IDatabaseApplicationLockService
    {
        Task AcquireTransactionExclusiveLocksAsync(IReadOnlyCollection<string> lockResourceNames);
        Task<IAsyncDisposable?> TryAcquireSessionExclusiveLockWithoutWaitingAsync(string lockResourceName);
    }
}
