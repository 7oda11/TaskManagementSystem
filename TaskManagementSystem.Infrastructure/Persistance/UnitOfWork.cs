using TaskManagementSystem.Core.Interfaces;
using TaskManagementSystem.Infrastructure.Persistance.Data;

namespace TaskManagementSystem.Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _context;

        // ── Cache: stores created repositories by their type ───────
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(ApplicationDBContext context)
        {
            _context = context;
        }

        // ── Returns existing repo or creates one on first access ───
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
                _repositories[type] = new Repository<TEntity>(_context);

            return (IRepository<TEntity>)_repositories[type];
        }

        // ── Save ───────────────────────────────────────────────────
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);

        public void Dispose()
        {
            _repositories.Clear();
            _context.Dispose();
        }
    }
}
