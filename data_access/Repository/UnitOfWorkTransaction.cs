using business_logic.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace data_access.Repository
{
    public sealed class UnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _databaseTransaction;

        public UnitOfWorkTransaction(IDbContextTransaction databaseTransaction)
        {
            _databaseTransaction = databaseTransaction;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return _databaseTransaction.CommitAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _databaseTransaction.DisposeAsync();
        }
    }
}
