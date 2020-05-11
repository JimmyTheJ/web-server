using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VueServer.Models.Modules;
using VueServer.Models.Library;
using VueServer.Models.User;
using VueServer.Models.Directory;

namespace VueServer.Models.Context
{
    public interface IWSContext : IDisposable
    {
        #region -> Tables

        DbSet<Notes> Notes { get; set; }

        DbSet<Weight> Weight { get; set; }

        DbSet<TranscodeFile> TranscodeFiles { get; set; }

        #region -> Modules

        DbSet<ModuleAddOn> Modules { get; set; }

        DbSet<UserHasModuleAddOn> UserHasModule { get; set; }

        #endregion

        #region -> Library 

        DbSet<Author> Authors { get; set; }

        DbSet<Book> Books { get; set; }

        DbSet<Bookcase> Bookcases { get; set; }

        DbSet<Genre> Genres { get; set; }

        DbSet<Series> Series { get; set; }

        DbSet<Shelf> Shelves { get; set; }

        DbSet<BookAuthor> BookHasAuthors { get; set; }

        DbSet<BookGenre> BookHasGenres { get; set; }

        #endregion

        #region -> Identity

        DbSet<WSUser> Users { get; set; }
        DbSet<WSRole> Roles { get; set; }
        DbSet<WSUserInRoles> UserRoles { get; set; }
        DbSet<WSUserLogin> UserLogin { get; set; }
        DbSet<WSUserTokens> UserTokens { get; set; }

        #endregion

        #endregion

        #region -> Functions

        void BeginTransaction();
        void Commit();
        void Rollback();

        int SaveChanges ();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity) where TEntity : class;

        EntityEntry Entry([NotNull] object entity);

        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        EntityEntry<TEntity> Remove<TEntity>([NotNull] TEntity entity) where TEntity : class;

        #endregion
    }
}
