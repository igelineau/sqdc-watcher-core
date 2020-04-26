using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.DataAccess
{
    public class SqdcDataAccess
    {
        private readonly ILogger<SqdcDataAccess> logger;
        private const string DEFAULT_APPSTATE_KEY = "default";
        const bool DROP_TABLES = false;
        
        private IDbConnection db;

        public SqdcDataAccess(IDbConnectionFactory connectionFactory, ILogger<SqdcDataAccess> logger)
        {
            this.logger = logger;

            db = connectionFactory.OpenDbConnection();

            string dataObjectsNamespace = typeof(Product).Namespace ?? throw new InvalidOperationException("Product has no namespace.");
            var tablesTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.CustomAttributes.Any(ca => ca.AttributeType == typeof(TableObjectAttribute)))
                .ToArray();
            OrmLiteConfig.DialectProvider = new SqliteOrmLiteDialectProvider();
            db.CreateTables(overwrite: DROP_TABLES, tablesTypes);
        }
        
        public DateTime? GetLastProductsListUpdateTimestamp()
        {
            var q = db.From<AppState>().Limit(1).Select<AppState>(s => s.LastProductsListRefresh);
            var result = db.Select(q).FirstOrDefault();
            return result?.LastProductsListRefresh;
        }

        public void SetLastProductsListUpdateTimestamp(DateTime value)
        {
            var dataToUpdate = new {LastProductsListRefresh = value};
            int affected = db.Update<AppState>(dataToUpdate, appState => appState.Key == DEFAULT_APPSTATE_KEY);
            if (affected == 0)
            {
                db.Insert(new AppState {Key = DEFAULT_APPSTATE_KEY, LastProductsListRefresh = value});
            }
        }

        public void InsertHistoryEntry(StockHistory stockHistory)
        {
            db.Insert(stockHistory);
        }

        public void InsertPriceHistoryEntry(PriceHistory priceHistory)
        {
            db.Insert(priceHistory);
        }
    }
}