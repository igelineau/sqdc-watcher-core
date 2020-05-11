#region

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;
using XFactory.SqdcWatcher.Data.Entities;

#endregion

namespace XFactory.SqdcWatcher.DataAccess
{
    public class SqdcDbContext : DbContext
    {
        private readonly IHostEnvironment hostingEnvironment;
        private string connectionString;
        private const string ConfigDirName = "sqdc-watcher";

        public SqdcDbContext()
        {
        }

        public SqdcDbContext(DbContextOptions<SqdcDbContext> options, IHostEnvironment hostingEnvironment) : base(options)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductVariant> ProductVariants { get; set; }

        public DbSet<SpecificationAttribute> SpecificationAttribute { get; set; }

        public DbSet<AppState> AppState { get; set; }

        public DbSet<StockHistory> StockHistory { get; set; }

        public DbSet<PriceHistory> PriceHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite(GetConnectionString())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        public string GetConnectionString()
        {
            if (connectionString != null)
            {
                return connectionString;
            }
            
            string envName = hostingEnvironment.EnvironmentName;
            string envSuffix = "";
            if (!hostingEnvironment.IsProduction())
            {
                envSuffix = "_" + envName;
            }

            string databasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ConfigDirName,
                $"store{envSuffix}.db");

            if (!Directory.Exists(Path.GetDirectoryName(databasePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
            }

            connectionString = $"Data Source={databasePath}";
            return connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //This will singularize all table names
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.DisplayName());
            }
            
            modelBuilder.Entity<Product>()
                .Ignore(p => p.IsNew)
                .Property(p => p.Id).ValueGeneratedNever();

            modelBuilder.Entity<ProductVariant>()
                .Ignore(pv => pv.MetaData)
                .Property(pv => pv.Id).ValueGeneratedNever();
        }
    }
}