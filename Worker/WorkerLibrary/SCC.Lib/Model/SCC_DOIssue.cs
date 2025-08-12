using System;

namespace SCCWorker.Model
{
    public class SCC_DOIssue
    {
        public string Line { get; set; } = string.Empty;
        public string MerchID { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int DemandQty { get; set; }
        public DateTime PromisedDeliveryDate { get; set; }
        public string FactoryCode { get; set; }
    }
}
