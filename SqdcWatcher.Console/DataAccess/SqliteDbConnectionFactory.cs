#region

using System.Data;
using System.Data.SQLite;
using ServiceStack.Data;

#endregion

namespace SqdcWatcher.DataAccess
{
    public class SqliteDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string connectionString;

        public SqliteDbConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection OpenDbConnection()
        {
            IDbConnection connection = CreateDbConnection();
            connection.Open();
            return connection;
        }

        public IDbConnection CreateDbConnection() => new SQLiteConnection(connectionString);
    }
}