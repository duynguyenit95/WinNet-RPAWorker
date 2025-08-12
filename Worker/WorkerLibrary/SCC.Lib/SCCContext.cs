using SCCWorker.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCC.Lib
{
    public class SCCContext : DbContext
    {
        public SCCContext() : base("name=ORPEntities")
        {
            
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
        }

        public virtual DbSet<T_SCC_Order> T_SCC_Orders { get; set; }
        public virtual DbSet<T_SCC_Pilot1_OrderInfo> T_SCC_Pilot1_OrderInfos { get; set; }
        public virtual DbSet<T_SCC_Pilot1_OrderInfoDetail> T_SCC_Pilot1_OrderInfoDetails { get; set; }
        public virtual DbSet<T_SCC_LogHistory> T_SCC_LogHistories { get; set; }
        public virtual DbSet<T_SCC_Pilot2_OrderInfo> T_SCC_Pilot2_OrderInfos { get; set; }
    }
}
