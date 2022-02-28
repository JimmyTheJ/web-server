using Microsoft.Data.Sqlite;
using System;
using VueServer.Modules.Core.Context;

namespace VueServer.Test.Integration.Fixture
{
    public class SqliteDBFixture : IDisposable
    {
        private readonly SqliteConnection connection;

        public SqliteDBFixture()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            Context = new SqliteWSContext(connection);
            Context.Database.EnsureCreated();
        }

        public WSContext Context { get; set; }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
