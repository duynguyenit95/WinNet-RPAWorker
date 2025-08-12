using RPA.Core;
using RPA.Worker.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPA112.Lib
{
    public class RPA112Data
    {
        public string SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string CustomerName2 { get; set; }
        public string CustomerNo { get; set; }
        public string StyleNo { get; set; }
        public string DocumentNo { get; set; }
        public string ShortText { get; set; }
        public string ColorDescription { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime RequiredDeliveryDate { get; set; }
        public decimal PlannedQuantity { get; set; }
        public string UnitNo { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime SupplierConfirmDeliveryDate { get; set; }
        public decimal AccumulatedReceivedQuantity { get; set; }
        public decimal LackQuantity { get; set; }
        public string MaterialProcess1 { get; set; }
        public DateTime ExpectedComeInFactoryDate { get; set; }
        public DateTime ProductionPlanDate { get; set; }
        public string Season { get; set; }
        public string AddressID { get; set; }
        public string VendorName { get; set; }
        public string Emails { get; set; }
    }
}
