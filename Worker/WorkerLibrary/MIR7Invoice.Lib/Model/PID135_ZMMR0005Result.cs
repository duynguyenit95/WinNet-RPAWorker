using System;
using System.ComponentModel.DataAnnotations;

namespace MIR7InvoiceWorker.Model
{
    public class PID135_ZMMR0005Result
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string SupplierId { get; set; }
        public string Buyer { get; set; }
        public string RequestNo { get; set; }
        public string PO { get; set; }
        public string POItem { get; set; }
        public string PlannedTrip { get; set; }
        public string Material { get; set; }
        public string MaterialDescription { get; set; }
        public string SO { get; set; }
        public int? POItemIndex { get; set; }
        public string GridDescription { get; set; }
        public string Unit { get; set; }
        public decimal? Quantity { get; set; }
        public string Currency { get; set; }
        public decimal? Price { get; set; }
        public int? PriceUnit { get; set; }
        public decimal? Amount { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
