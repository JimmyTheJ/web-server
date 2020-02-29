using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VueServer.Models.User;

namespace VueServer.Models.Context
{
    public interface IWSContext : IDisposable
    {
        #region -> Tables

        DbSet<Notes> Notes { get; set; }

        DbSet<Weight> Weight { get; set; }

        DbSet<WSUser> Users { get; set; }
        DbSet<WSRole> Roles { get; set; }
        DbSet<WSUserInRoles> UserRoles { get; set; }
        DbSet<WSUserLogin> UserLogin { get; set; }
        DbSet<WSUserTokens> UserTokens { get; set; }

        #endregion

        #region -> Functions

        int SaveChanges ();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity) where TEntity : class;

        EntityEntry Entry([NotNull] object entity);

        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        EntityEntry<TEntity> Remove<TEntity>([NotNull] TEntity entity) where TEntity : class;

        #endregion
    }
}
