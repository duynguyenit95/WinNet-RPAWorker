using MIR7InvoiceWorker.Model;
using RPA.Core.Models.SAP.ZACCOUNT;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIR7Invoice.Lib
{
    public class MIR7InvoiceContext : DbContext
    {
        public MIR7InvoiceContext() : base("name=ORPEntities")
        {
            
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
        }

        public virtual DbSet<PID135_InvoiceResult> PID135_InvoiceResults { get; set; }
        public virtual DbSet<PID135_ZMMR0005Result> PID135_ZMMR0005Results { get; set; }
        public virtual DbSet<ZAccountResult> ZAccountResults { get; set; }
    }
}
