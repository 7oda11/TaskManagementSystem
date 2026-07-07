using System.Linq.Expressions;
using TaskManagementSystem.Core.Models;

namespace TaskManagementSystem.Core.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        // ── Queries ────────────────────────────────────────────────
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllByConditionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllPaginatedByConditionAsync(
                                       Expression<Func<TEntity, bool>> predicate,
                                       int pageNumber,
                                       int pageSize,
                                       CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllWithOptionsAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetAllWithOptionsPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);

        // ── Specification Methods ─────────────────────────────────
        Task<TEntity?> GetWithSpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllWithSpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
        Task<PagedResult<TEntity>> GetPagedWithSpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
        Task<int> CountWithSpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> CountByConditionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        // ── Commands (no SaveChanges here anymore) ─────────────────
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
        void DeleteRange(IEnumerable<TEntity> entities);
    }
}
