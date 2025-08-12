using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCCWorker.Model
{
    public class T_SCC_Pilot1_OrderInfoDetail
    {
        [Key]
        public int Id { get; set; }
        public DateTime NDC { get; set; }
        public int Quantity { get; set; }
        public Guid InfoGuid { get; set; }
    }
}
