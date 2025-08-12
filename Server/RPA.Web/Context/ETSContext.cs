using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RPA.Core.Data
{
    public static class ETSContextExtension
    {
        public static async Task<CutLineData> GetCutLineData(this ETSContext context
            , string workline
            , string nightWorkline
            , DateTime workDate)
        {
            string query = @"DECLARE @WorkDate date = '" + workDate.ToString("yyyy-MM-dd") + @"';
                            DECLARE @Workline nvarchar(16) = '" + workline + @"';
                            DECLARE @NightWorkline nvarchar(16) = '" + nightWorkline + @"';
                            with t0 as(
	                            select @WorkDate WorkDate,@Workline Workline
                            ),
                            t1 as(
                            select @Workline WorkLine,Sum(RealMinute) as TotalOffWorkInMinute
                            from TBOFFSTANDRECORD (nolock) ta   
                            where BillDate = @WorkDate
                            and (WorkLine = @Workline OR WorkLine = @NightWorkline)
                            ),
                            t2 as(
                            select @Workline WorkLine,Sum(TotalQty) as TotalOutput
                            from EmployeeEfficencyInfo(nolock)
                            where (WorkLine = @Workline OR Workline = @NightWorkline) 
                            and Work_Date = @WorkDate
                            )
                            select t0.*,isnull(TotalOffWorkInMinute,0) as TotalOffWorkInMinute,isnull(TotalOutput,0) as TotalOutput
                            from t0
                            left join t1 on t1.WorkLine = t0.Workline
                            left join t2 on t2.WorkLine = t0.Workline";
           var result =  await context.CutLineDatas.FromSqlRaw(query).ToListAsync();
           return result.FirstOrDefault();
        }
    }

    public class ETSContext : DbContext
    {
        public ETSContext(DbContextOptions<ETSContext> options) : base(options)
        {
        }

        public static DbContextOptions<ETSContext> UseConnectionString(string con)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ETSContext>();
            optionsBuilder.UseSqlServer(con);
            return optionsBuilder.Options;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<CutLineData>().HasNoKey();
        }

        public DbSet<CutLineData> CutLineDatas { get; set; }
    }
    public class CutLineData
    {
        public DateTime WorkDate { get; set; }
        public string Workline { get; set; }
        public decimal TotalOutput { get; set; }
        public decimal TotalOffWorkInMinute { get; set; }
    }
}
