using RPA.Core;
using RPA.Tools;
using RPA.Worker.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPA112.Lib
{
    public class RPA112Context : DbContext
    {
        public const string DB_NAME = "RPA112.sqlite";
        public RPA112Context() : base(new SQLiteConnection()
        {
            ConnectionString = new SQLiteConnectionStringBuilder()
            { DataSource = DB_NAME, ForeignKeys = true }
            .ConnectionString
        }, true)
        { }
        public DbSet<POAddress> POAddresses { get; set; }
    }

    public class RPA112DBInitializer : IDatabaseInitializer<RPA112Context>
    {
        public void InitializeDatabase(RPA112Context context)
        {

        }
    }

    public class ContextDBConfiguration : SQLiteConfiguration
    {
        public ContextDBConfiguration() : base()
        {

        }
    }

    public class POAddress
    {
        [Key]
        [MaxLength(32)]
        public string PO { get; set; }
        [MaxLength(32)]
        public string AddressID { get; set; }

        [MaxLength(1024)]
        public string VendorName { get; set; }
    }
}
