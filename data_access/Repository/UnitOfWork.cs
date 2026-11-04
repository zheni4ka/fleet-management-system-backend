using business_logic.Interfaces;
using data_access.data;

namespace data_access.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FMS_DbContext _context;

        public UnitOfWork(FMS_DbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
