using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
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

        public async Task<IEnumerable<TEntity>> GetAllAsync()
            => await _entity.AsNoTracking().ToListAsync();

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            var entity = await _entity.FindAsync(id);
            if (entity == null)
                throw new Exception($"Entity with id {id} was not found.");
            return entity;
        }

        public async Task<TEntity?> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate)
            => await _entity.AsNoTracking().FirstOrDefaultAsync(predicate);

        public async Task<IEnumerable<TEntity>> GetAllByConditionAsync(Expression<Func<TEntity, bool>> predicate)
            => await _entity.AsNoTracking().Where(predicate).ToListAsync();

        public async Task<IEnumerable<TEntity>> GetAllPaginatedAsync(int pageNumber, int pageSize)
            => await _entity.AsNoTracking()
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync();

        public async Task<IEnumerable<TEntity>> GetAllPaginatedByConditionAsync(
            Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize)
            => await _entity.AsNoTracking()
                   .Where(predicate)
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync();

        public async Task<IEnumerable<TEntity>> GetAllWithOptionsAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes)
            => await BuildQuery(predicate, orderBy, includes).ToListAsync();

        public async Task<IEnumerable<TEntity>> GetAllWithOptionsPaginatedAsync(
            int pageNumber, int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            params Expression<Func<TEntity, object>>[] includes)
            => await BuildQuery(predicate, orderBy, includes)
                   .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync();

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
            => await _entity.AsNoTracking().AnyAsync(predicate);

        public async Task<int> CountAsync()
            => await _entity.AsNoTracking().CountAsync();

        public async Task<int> CountByConditionAsync(Expression<Func<TEntity, bool>> predicate)
            => await _entity.AsNoTracking().CountAsync(predicate);

        // ── Commands (only track changes, no SaveChanges) ──────────

        public async Task AddAsync(TEntity entity)
            => await _entity.AddAsync(entity);   // no SaveChanges ✅

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
            => await _entity.AddRangeAsync(entities);  // no SaveChanges ✅

        public void Update(TEntity entity)
            => _entity.Update(entity);   // sync, no SaveChanges ✅

        public void Delete(TEntity entity)
            => _entity.Remove(entity);   // sync, no SaveChanges ✅

        public async Task DeleteByIdAsync(int id)
        {
            var entity = await _entity.FindAsync(id);
            if (entity == null)
                throw new Exception($"Entity with id {id} was not found.");
            _entity.Remove(entity);      // no SaveChanges ✅
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
            => _entity.RemoveRange(entities);   // sync, no SaveChanges ✅
    }

}
