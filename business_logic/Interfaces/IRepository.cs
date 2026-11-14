using Ardalis.Specification;

namespace business_logic.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(object id);
        Task InsertAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);

        Task<TEntity?> GetItemBySpecAsync(ISpecification<TEntity> specification);
        Task<IEnumerable<TEntity>> GetListBySpecAsync(ISpecification<TEntity> specification);
        Task<bool> AnyBySpecAsync(ISpecification<TEntity> specification);
        Task<int> CountBySpecAsync(ISpecification<TEntity> specification);
        Task<IReadOnlyList<TResult>> GetProjectedListBySpecAsync<TResult>(ISpecification<TEntity, TResult> specification);
        Task<TResult?> GetProjectedItemBySpecAsync<TResult>(ISpecification<TEntity, TResult> specification);
    }
}
