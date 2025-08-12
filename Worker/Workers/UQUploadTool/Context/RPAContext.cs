using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIReport_ORPContext
{
    public class RPAContext : DbContext
    {
        public RPAContext() : base("name=RPAEntities")
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
