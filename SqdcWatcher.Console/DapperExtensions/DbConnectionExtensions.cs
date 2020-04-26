#region

using System;
using System.Data;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

#endregion

namespace SqdcWatcher.DapperExtensions
{
    public static class DbConnectionExtensions
    {
        public static async Task InsertOrUpdate<T>(this IDbConnection connection, T entity, bool update) where T : class
        {
            if (update)
            {
                bool updated = await connection.UpdateAsync(entity);
                if (!updated)
                {
                    throw new Exception("updated 0 record");
                }
            }
            else
            {
                await connection.InsertAsync(entity);
            }
        }
    }
}