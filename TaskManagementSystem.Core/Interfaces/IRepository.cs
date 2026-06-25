using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace TaskManagementSystem.Core.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        // ── Queries ────────────────────────────────────────────────
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(int id);
        Task<TEntity?> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllByConditionAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllPaginatedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<TEntity>> GetAllPaginatedByConditionAsync(
                                       Expression<Func<TEntity, bool>> predicate,
                                       int pageNumber,
                                       int pageSize);
        Task<IEnumerable<TEntity>> GetAllWithOptionsAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetAllWithOptionsPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountByConditionAsync(Expression<Func<TEntity, bool>> predicate);

        // ── Commands (no SaveChanges here anymore) ─────────────────
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task DeleteByIdAsync(int id);
        void DeleteRange(IEnumerable<TEntity> entities);
    }

}
