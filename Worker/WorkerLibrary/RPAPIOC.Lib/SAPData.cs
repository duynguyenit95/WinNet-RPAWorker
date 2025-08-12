
using System.Collections.Generic;
using System.Linq;

namespace RPAPIOC.Lib
{
    public class SAPData
    {
        public string PO { get; set; }
        public string FabricNo { get; set; }
        public decimal TotalAmount => Items.Sum(y => y.Colors.Sum(x => x.Amount));
        public List<PIOCItem> Items { get; set; } = new List<PIOCItem>();
    }
}
