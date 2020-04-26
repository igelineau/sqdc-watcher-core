#region

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using XFactory.SqdcWatcher.Data.Entities;

#endregion

namespace XFactory.SqdcWatcher.DataAccess
{
    public class SqdcDbContext : DbContext
    {
        private const string ConfigDirName = "sqdc-watcher";

        private static readonly string ConnectionString;

        static SqdcDbContext()
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string envSuffix = "";
            if (envName != null)
            {
                envSuffix = "_" + envName;
            }

            string databasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ConfigDirName,
                $"store{envSuffix}.db");

            Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "PRODUCTION"}");
            Console.WriteLine($"Using database : {databasePath}");

            if (!Directory.Exists(Path.GetDirectoryName(databasePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
            }

            ConnectionString = $"Data Source={databasePath}";
        }

        public SqdcDbContext()
        {
        }

        public SqdcDbContext(DbContextOptions<SqdcDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<SpecificationAttribute> SpecificationAttributes { get; set; }
        public DbSet<AppState> AppState { get; set; }
        public DbSet<StockHistory> StockHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite(ConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Ignore(p => p.IsNew)
                .Property(p => p.Id).ValueGeneratedNever();

            modelBuilder.Entity<ProductVariant>()
                .Ignore(pv => pv.MetaData)
                .Property(pv => pv.Id).ValueGeneratedNever();
        }
    }
}