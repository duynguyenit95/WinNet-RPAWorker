using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RPA.Web.Models;
using MIR7InvoiceWorker.Model;

namespace RPA.Core.Data
{
    public class RPAInvoiceContext : DbContext
    {
        public RPAInvoiceContext(DbContextOptions<RPAInvoiceContext> options) : base(options)
        {
        }

        public static DbContextOptions<RPAInvoiceContext> UseConnectionString(string con)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RPAInvoiceContext>();
            optionsBuilder.UseSqlServer(con);
            return optionsBuilder.Options;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntityConfigure<>).Assembly); // Here UseConfiguration is any IEntityTypeConfiguration
        }
        public DbSet<PID135_InvoiceResult> PID135_InvoiceResults { get; set; }
        public DbSet<PID135_ZMMR0005Result> PID135_ZMMR0005Results { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }

}
