namespace data_access.Repository
{
    public static class SqlServerApplicationLockValues
    {
        public const int LockTimeout = -1;
        public const string ExclusiveLockMode = "Exclusive";
        public const string TransactionLockOwner = "Transaction";
        public const string SessionLockOwner = "Session";
        public const int NoWaitLockTimeoutInMilliseconds = 0;
    }
}
