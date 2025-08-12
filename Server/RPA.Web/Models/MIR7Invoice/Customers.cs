using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIR7InvoiceWorker.Model
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string FormRecognizerModel { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public bool IsActive { get; set; }
        public bool CanRemoveOrDisable { get; set; }
        public string PIC { get; set; }
        public string SAPID { get; set; }
    }
}
