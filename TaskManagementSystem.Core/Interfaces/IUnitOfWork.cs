using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManagementSystem.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // ── Dynamic repository access ──────────────────────────────
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;

        // ── Save ───────────────────────────────────────────────────
        Task<int> SaveChangesAsync();
    }
}
