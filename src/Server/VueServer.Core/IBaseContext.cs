using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace VueServer.Core
{
    public interface IBaseContext : IDisposable
    {
        #region -> Functions

        void BeginTransaction();
        void Commit();
        void Rollback();

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity) where TEntity : class;

        EntityEntry Entry([NotNull] object entity);

        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        EntityEntry<TEntity> Remove<TEntity>([NotNull] TEntity entity) where TEntity : class;

        #endregion
    }
}
