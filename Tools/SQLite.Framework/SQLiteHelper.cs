using System.Collections.Generic;
using System.Data.SQLite;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Data.SQLite.EF6;
using System.Data.Entity.Core.Common;
using System.Threading.Tasks;
using System.Linq;
using System.Data.SQLite.EF6.Migrations;

namespace RPA.Tools
{
    public class SQLiteHelper
    {
        public static bool DbExist(string dbFile)
        {
            return System.IO.File.Exists(dbFile);
        }

        public static void CreateDataBase(string dbFile)
        {
            SQLiteConnection.CreateFile(dbFile);
        }
        public static void MigrationAndInit<TContext, TInitializer>() where TContext : DbContext, new()
                                                            where TInitializer : IDatabaseInitializer<TContext>, new()
        {
            var configuration = new EFSQLiteConfiguration<TContext>();
            configuration.ContextType = typeof(TContext);
            var migrator = new DbMigrator(configuration);
            migrator.Update();
            var init = new TInitializer();
            init.InitializeDatabase(new TContext());
        }

        public static void CreateDatabaseIfNotExists<TContext, TInitializer>(string dbFile) where TContext : DbContext, new()
                                                            where TInitializer : IDatabaseInitializer<TContext>, new()
        {
            if (!DbExist(dbFile))
            {
                CreateDataBase(dbFile);
                MigrationAndInit<TContext, TInitializer>();
            }
        }
    }

    public class EFSQLiteConfiguration<TContext> : DbMigrationsConfiguration<TContext> where TContext : DbContext
    {
        public EFSQLiteConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator());
        }
    }

    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetProviderServices("System.Data.SQLite", (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
        }
    }

    public class SampleDbContext : DbContext
    {
        public const string DB_NAME = "SampleDb.sqlite";
        public SampleDbContext() : base(new SQLiteConnection()
                                            {
                                                ConnectionString = new SQLiteConnectionStringBuilder() 
                                                                    { DataSource = DB_NAME, ForeignKeys = true }
                                                                    .ConnectionString
                                            }, true){}
        public DbSet<SampleClass> Samples { get; set; }
    }

    public class SampleClass
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
    }

    public class SampleDBInitializer : IDatabaseInitializer<SampleDbContext>
    {
        public void InitializeDatabase(SampleDbContext context)
        {
            if (!context.Samples.Any())
            {
                IList<SampleClass> defaultStandards = new List<SampleClass>();

                defaultStandards.Add(new SampleClass() { Name = "First" });
                defaultStandards.Add(new SampleClass() { Name = "Second" });
                defaultStandards.Add(new SampleClass() { Name = "Third" });

                context.Samples.AddRange(defaultStandards);
                context.SaveChanges();
            }
        }
    }

}
