using Ardalis.Specification;
using System.Linq.Expressions;

namespace business_logic.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task InsertAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);

        Task<TEntity?> GetItemBySpecAsync(ISpecification<TEntity> specification);
        Task<IEnumerable<TEntity>> GetListBySpecAsync(ISpecification<TEntity> specification);
        Task<bool> AnyBySpecAsync(ISpecification<TEntity> specification);
        Task<int> CountBySpecAsync(ISpecification<TEntity> specification);
        Task<IReadOnlyDictionary<TGroupKey, int>> GetCountsByGroupKeyBySpecAsync<TGroupKey>(
            ISpecification<TEntity> specification,
            Expression<Func<TEntity, TGroupKey>> groupKeySelector) where TGroupKey : notnull;
        Task<IReadOnlyList<TResult>> GetProjectedListBySpecAsync<TResult>(ISpecification<TEntity, TResult> specification);
        Task<TResult?> GetProjectedItemBySpecAsync<TResult>(ISpecification<TEntity, TResult> specification);
        IQueryable<TResult> GetProjectedQueryBySpec<TResult>(ISpecification<TEntity, TResult> specification);
    }
}
