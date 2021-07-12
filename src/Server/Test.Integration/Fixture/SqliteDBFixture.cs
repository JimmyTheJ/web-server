using Microsoft.Data.Sqlite;
using System;
using VueServer.Models.Context;

namespace VueServer.Test.Integration.Fixture
{
    public class SqliteDBFixture : IDisposable
    {
        private SqliteConnection connection;

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
