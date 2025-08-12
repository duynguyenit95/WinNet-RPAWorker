using Microsoft.EntityFrameworkCore;
using RPA.Web.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace RPA.Web.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }

        public static DbContextOptions<UserContext> UseConnectionString(string con)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserContext>();
            optionsBuilder.UseSqlServer(con);
            return optionsBuilder.Options;
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<HR_Org> HR_Orgs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasKey(c => c.EmpId);
            modelBuilder.Entity<HR_Org>().HasNoKey();
        }
    }

    [Table("Employee")]
    public class Employee
    {
        [Key]
        [Column("Emp_id")]
        public string EmpId { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        [Column("Emp_name")]
        public string EmpName { get; set; }
        public string Card_no { get; set; }
        public string Pos_id { get; set; }
        public string Pos_name { get; set; }
        public string Sex { get; set; }
        public string Factory { get; set; }
        public string Section { get; set; }
        public string Dept { get; set; }
        public string Workshop { get; set; }
        [Column("Line_no")]
        public string LineNo { get; set; }
        public bool? Status { get; set; }
    }

    [Table("HR_Org")]
    public class HR_Org
    {
        [Column("Fac_no")]
        public string Factory { get; set; }
        [Column("Sec_no")]
        public string Section { get; set; }
        [Column("Dep_no")]
        public string Department { get; set; }
        [Column("Wrk_no")]
        public string Workshop { get; set; }
        [Column("Lin_no")]
        public string LineNo { get; set; }
    }

    public class UserAccesses
    {
        public List<string> Accesses { get; set; }
        public List<MenuItem> Menus { get; set; }
        public bool isAdmin { get; set; }
    }
    public class MenuItem
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string Type { get; set; }
        public int Priority { get; set; }
        public int ParentID { get; set; }
        public int ID { get; set; }
    }
}
