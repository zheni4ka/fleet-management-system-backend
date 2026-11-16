namespace business_logic.Interfaces
{
    public interface IUnitOfWorkTransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
