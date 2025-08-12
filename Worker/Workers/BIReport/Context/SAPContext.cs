using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIReportContext
{
    public class SAPContext : DbContext
    {
        public SAPContext() : base("name=SAPEntities")
        {
            
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }

        public virtual DbSet<T_R_SOInfor> T_R_SOInfors { get; set; }
        public virtual DbSet<T_R_SOLink> T_R_SOLinks { get; set; }
        public virtual DbSet<T_R_SOMOData> T_R_SOMODatas { get; set; }

        [Table("T_R_SOInfors")]
        public class T_R_SOInfor
        {
            [Key]
            public int ID { get; set; }
            public string SO { get; set; }
            public int SOItemNo { get; set; }
            public string Factory { get; set; }
            public string MO { get; set; }
            public string RejectionReason { get; set; }
            public string StyleNo { get; set; }
            public string Customer { get; set; }
            public string PO { get; set; }
            [NotMapped]
            public string POCode
            {
                get
                {
                    return PO.Split('<').FirstOrDefault();
                }
            }
            public DateTime? ExportDate { get; set; }
            public decimal Quantity { get; set; }
            public DateTime UpdateTime { get; set; }
            public string Color { get; set; }
            public string ZHCOLORDESC { get; set; }
            public string DIVISION { get; set; }
            public string SHIPPINGTYPE { get; set; }
            public string INCOTERMS { get; set; }
            public string FLEXCOLOR { get; set; }
            public string ENCOLORDESC { get; set; }
            public string COLLECTION { get; set; }
            public string BUY_STRATEGY { get; set; }
            public string NEWOLDSTYLENO { get; set; }
            public string CUSMAT { get; set; }
            public string COUNTRY { get; set; }
            public DateTime? FLOORSETDATE { get; set; }
            public DateTime? ZUPDATEFACTORYDATE_YMD03 { get; set; }
            public string SEASON { get; set; }
            public string EnCustomerName { get; set; }
            public string CustomerCode { get; set; }
        }
        [Table("T_R_SOLinks")]
        public class T_R_SOLink
        {
            [Key]
            public int ID { get; set; }
            public string SO { get; set; }
            public int SOItemNo { get; set; }
            public string BigSO { get; set; }
            public int BigSOItemNo { get; set; }
            public DateTime UpdateTime { get; set; }
            public DateTime? POISSUEDATE { get; set; }
            public DateTime? PLANFABDATE { get; set; }
            public DateTime? PLANPADDATE { get; set; }
            public DateTime? PLANTRIMDATE { get; set; }
            public DateTime? ACTUALFABDATE { get; set; }
            public DateTime? ACTUALTRIMDATE { get; set; }
        }
        [Table("T_R_SOMODatas")]
        public class T_R_SOMOData
        {
            [Key]
            public int ID { get; set; }
            public string SO { get; set; }
            public int SOItemNo { get; set; }
            public string MO { get; set; }
            public string Factory { get; set; }
            public string OrderType { get; set; }
            public string Status { get; set; }
            public string StyleNo { get; set; }
        }
    }
}
