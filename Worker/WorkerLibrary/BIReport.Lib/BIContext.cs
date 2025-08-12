using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace BIReport.Lib
{
    public class BIContext : DbContext
    {
        public BIContext() : base("name=RPAEntities")
        {
            
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
        }
        public virtual DbSet<BIReport_UQUploadResult> BIReport_UQUploadResults { get; set; }

        [Table("BIReport_UQUploadResult")]
        public class BIReport_UQUploadResult
        {
            public int Id { get; set; }
            public string SewingActualPath { get; set; }
            public string ZipFileName { get; set; }
            public string Status { get; set; }
            public DateTime UpdateTime { get; set; }
            public DateTime UploadTime { get; set; }
        }

    }
}
