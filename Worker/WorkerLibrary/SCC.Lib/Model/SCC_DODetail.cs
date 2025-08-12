using System;

namespace SCCWorker.Model
{
    public class SCC_DODetail
    {
        public string DOId { get; set; } = string.Empty;
        public string DOLine { get; set; } = string.Empty;
        public string MerchID { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int CustomerOrderQty { get; set; }
        public int ConfirmedQty { get; set; }
        public DateTime CustomerRequirementDate { get; set; }
        public DateTime NDC { get; set; }
    }
}
