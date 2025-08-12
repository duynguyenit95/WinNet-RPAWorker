using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIR7InvoiceWorker.Model
{
    [Table("PID135_InvoiceResult")]
    public class PID135_InvoiceResult
    {
        [Key]
        public int Id { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string FileName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string FolderName { get; set; }
        public string InvoiceNo { get; set; }
        public string SAPInvoiceNo { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public decimal? SAPAmount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public int? InvoiceQuantity { get; set; }
        public int? SAPQuantity { get; set; }
        public string Result { get; set; }
        public string ResultType { get; set; }
        public DateTime TimeBegin { get; set; }
        public DateTime TimeEnd { get; set; }
        public string SAPServer { get; set; }
        public string SAPAccount { get; set; }
        public string Reference { get; set; }
    }
}
