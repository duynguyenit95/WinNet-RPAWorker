using Microsoft.EntityFrameworkCore;
using RPA.Core;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace RPA.Web.Context
{
    public class ORPContext : DbContext
    {
        public ORPContext(DbContextOptions<ORPContext> options) : base(options)
        {
        }

        public static DbContextOptions<ORPContext> UseConnectionString(string con)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ORPContext>();
            optionsBuilder.UseSqlServer(con);
            return optionsBuilder.Options;
        }

        //public DbSet<T_SCC_Pilot1_Order> T_SCC_Pilot1_Orders { get; set; }
        //public DbSet<T_SCC_Pilot1_OrderDetail> T_SCC_Pilot1_OrderDetails { get; set; }

    }

}
