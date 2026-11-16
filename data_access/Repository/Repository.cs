using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using business_logic.Interfaces;
using data_access.data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace data_access.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public Repository(FMS_DbContext context)
        {
            _dbSet = context.Set<TEntity>();
        }

        public async Task InsertAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<TEntity?> GetItemBySpecAsync(ISpecification<TEntity> specification)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TEntity>> GetListBySpecAsync(ISpecification<TEntity> specification)
        {
            return await ApplySpecification(specification).ToListAsync();
        }

        public async Task<bool> AnyBySpecAsync(ISpecification<TEntity> specification)
        {
            return await ApplySpecification(specification).AnyAsync();
        }

        public async Task<int> CountBySpecAsync(ISpecification<TEntity> specification)
        {
            return await ApplySpecificationCriteria(specification).CountAsync();
        }

        public async Task<IReadOnlyDictionary<TGroupKey, int>> GetCountsByGroupKeyBySpecAsync<TGroupKey>(
            ISpecification<TEntity> specification,
            Expression<Func<TEntity, TGroupKey>> groupKeySelector) where TGroupKey : notnull
        {
            return await ApplySpecificationCriteria(specification)
                .GroupBy(groupKeySelector)
                .Select(entityGroup => new { GroupKey = entityGroup.Key, EntitiesCount = entityGroup.Count() })
                .ToDictionaryAsync(groupCount => groupCount.GroupKey, groupCount => groupCount.EntitiesCount);
        }

        public async Task<IReadOnlyList<TResult>> GetProjectedListBySpecAsync<TResult>(ISpecification<TEntity, TResult> specification)
        {
            return await GetProjectedQueryBySpec(specification).ToListAsync();
        }

        public async Task<TResult?> GetProjectedItemBySpecAsync<TResult>(ISpecification<TEntity, TResult> specification)
        {
            return await GetProjectedQueryBySpec(specification).FirstOrDefaultAsync();
        }

        public IQueryable<TResult> GetProjectedQueryBySpec<TResult>(ISpecification<TEntity, TResult> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_dbSet, specification);
        }

        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_dbSet, specification);
        }

        private IQueryable<TEntity> ApplySpecificationCriteria(ISpecification<TEntity> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_dbSet, specification, evaluateCriteriaOnly: true);
        }
    }
}
