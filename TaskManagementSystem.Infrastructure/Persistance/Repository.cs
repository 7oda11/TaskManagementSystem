using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManagementSystem.Core.Models;
using TaskManagementSystem.Core.Interfaces;
using TaskManagementSystem.Infrastructure.Persistance.Data;

namespace TaskManagementSystem.Infrastructure.Persistance
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDBContext _context;
        private readonly DbSet<TEntity> _entity;

        public Repository(ApplicationDBContext context)
        {
            _context = context;
            _entity = _context.Set<TEntity>();
        }

        // ── Internal helper ────────────────────────────────────────

        private IQueryable<TEntity> BuildQuery(
            Expression<Func<TEntity, bool>>? predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
            Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _entity.AsNoTracking();

            foreach (var include in includes)
                query = query.Include(include);

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = orderBy(query);

            return query;
        }

        // ── Queries ────────────────────────────────────────────────

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _entity.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _entity.FindAsync([id], cancellationToken);
            if (entity == null)
                throw new Exception($"Entity with id {id} was not found.");
            return entity;
        }

        public async Task<TEntity?> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => await _entity.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

        public async Task<IEnumerable<TEntity>> GetAllByConditionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => await _entity.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

        public async Task<IEnumerable<TEntity>> GetAllPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
            => await _entity.AsNoTracking()
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync(cancellationToken);

        public async Task<IEnumerable<TEntity>> GetAllPaginatedByConditionAsync(
            Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
            => await _entity.AsNoTracking()
                   .Where(predicate)
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync(cancellationToken);

        public async Task<IEnumerable<TEntity>> GetAllWithOptionsAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes)
            => await BuildQuery(predicate, orderBy, includes).ToListAsync(cancellationToken);

        public async Task<IEnumerable<TEntity>> GetAllWithOptionsPaginatedAsync(
            int pageNumber, int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes)
            => await BuildQuery(predicate, orderBy, includes)
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync(cancellationToken);

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => await _entity.AsNoTracking().AnyAsync(predicate, cancellationToken);

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
            => await _entity.AsNoTracking().CountAsync(cancellationToken);

        public async Task<int> CountByConditionAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => await _entity.AsNoTracking().CountAsync(predicate, cancellationToken);

        // ── Specification Methods ─────────────────────────────────

        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
        {
            var query = _entity.AsNoTracking();

            if (spec.Includes != null)
            {
                foreach (var include in spec.Includes)
                {
                    query = query.Include(include);
                }
            }

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            return query;
        }

        public async Task<TEntity?> GetWithSpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetAllWithSpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<TEntity>> GetPagedWithSpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(spec);
            
            var totalRecords = await query.CountAsync(cancellationToken);
            
            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            var data = await query.ToListAsync(cancellationToken);

            return new PagedResult<TEntity>
            {
                Data = data,
                TotalRecords = totalRecords,
                Page = spec.IsPagingEnabled && spec.Take > 0 ? (spec.Skip / spec.Take) + 1 : 1,
                PageSize = spec.IsPagingEnabled ? spec.Take : totalRecords
            };
        }

        public async Task<int> CountWithSpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).CountAsync(cancellationToken);
        }

        // ── Commands (only track changes, no SaveChanges) ──────────

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await _entity.AddAsync(entity, cancellationToken);

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            => await _entity.AddRangeAsync(entities, cancellationToken);

        public void Update(TEntity entity)
            => _entity.Update(entity);

        public void Delete(TEntity entity)
            => _entity.Remove(entity);

        public async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _entity.FindAsync([id], cancellationToken);
            if (entity == null)
                throw new Exception($"Entity with id {id} was not found.");
            _entity.Remove(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
            => _entity.RemoveRange(entities);
    }
}
