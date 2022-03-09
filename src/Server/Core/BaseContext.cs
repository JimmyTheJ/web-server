using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace VueServer.Core
{
    public abstract class BaseContext : DbContext, IBaseContext
    {
        private IDbContextTransaction _transaction;

        public BaseContext() { }

        public BaseContext(DbContextOptions options) : base(options) { }

        #region -> Functions

        public void BeginTransaction()
        {
            _transaction = Database.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                SaveChanges();
                _transaction.Commit();
            }
            finally
            {
                _transaction.Dispose();
            }
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }

        #endregion
    }
}
