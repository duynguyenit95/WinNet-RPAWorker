using System;
using System.ComponentModel.DataAnnotations;

namespace SCCWorker.Model
{
    public class T_SCC_Pilot1_OrderInfo
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string PO { get; set; }
        public string MerchID { get; set; }
        public string Line { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public DateTime PlannedNDC { get; set; }
        public int? PlannedQty { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid Guid { get; set; }
    }
}
