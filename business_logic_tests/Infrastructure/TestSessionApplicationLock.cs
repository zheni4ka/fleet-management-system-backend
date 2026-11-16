namespace business_logic_tests.Infrastructure
{
    public sealed class TestSessionApplicationLock : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
