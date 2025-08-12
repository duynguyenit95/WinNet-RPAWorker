using System;
using System.ComponentModel.DataAnnotations;

namespace SCCWorker.Model
{
    public class T_SCC_Pilot2_OrderInfo
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string PO { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
    }
}
