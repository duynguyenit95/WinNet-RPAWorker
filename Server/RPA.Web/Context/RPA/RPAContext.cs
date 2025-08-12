using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RPA.Web.Models;

namespace RPA.Core.Data
{
    public class RPAContext : DbContext
    {
        public RPAContext(DbContextOptions<RPAContext> options) : base(options)
        {
        }

        public static DbContextOptions<RPAContext> UseConnectionString(string con)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RPAContext>();
            optionsBuilder.UseSqlServer(con);
            return optionsBuilder.Options;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntityConfigure<>).Assembly); // Here UseConfiguration is any IEntityTypeConfiguration
        }
        public DbSet<QueueTask> QueueTasks { get; set; }
        public DbSet<RegexInfor> RegexInfors { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<FormRecognizerLog> FormRecognizerLogs { get; set; }
        public DbSet<WorkerInfor> WorkerInfors { get; set; }
        public DbSet<WorkerConfiguration> WorkerConfigurations { get; set; }
        public DbSet<RequestLog> RequestLogs { get; set; }
    }

}
