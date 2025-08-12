using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BIReportContext.SAPContext;

namespace BIReport_ORPContext
{
    public class ORPContext : DbContext
    {
        public ORPContext() : base("name=ORPEntities")
        {
            
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
        }
        public virtual DbSet<T_C_MODailyOutput_Workline> T_C_MODailyOutput_Worklines { get; set; }

        [Table("T_C_MODailyOutput_Workline")]
        public class T_C_MODailyOutput_Workline
        {
            public string Server { get; set; }
            public DateTime WorkDate { get; set; }
            public string Workline { get; set; }
            public string ZDCode { get; set; }
            public string COLOR_NO { get; set; }
            public string ColorName { get; set; }
            public string Size { get; set; }
            public int GxNo { get; set; }
            public decimal? TotalQty { get; set; }
            public decimal? TotalSam { get; set; }
            public DateTime? UpdateTime { get; set; }
            [Key]
            public int ID { get; set; }
        }

    }
}
