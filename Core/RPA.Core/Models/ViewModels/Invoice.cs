using System;
using System.Collections.Generic;

namespace RPA.Core.Data
{
    public class Invoice 
    {
        public string DocType { get; set; }
        public decimal DocTypeConfidence { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Quantity { get; set; }
        public List<string> PO { get; set; } = new List<string>();
        public string Currency { get; set; }
        public List<string> Delivery { get; set; } = new List<string>();
        public string CustomerRef { get; set; }
        public List<InvoiceDetails> InvoiceTable { get; set; } = new List<InvoiceDetails>();
    }

    public class InvoiceDetails
    {
        public int ItemNo { get; set; }
        public string PO { get; set; }
        public string MaterialNo { get; set; }
        public string Color { get; set; }
        public string BatchNo { get; set; }
        public decimal DeliveryLength { get; set; }
        public decimal UsableLength { get; set; }
        public string Unit { get; set; }
        public int PCS { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }
}
