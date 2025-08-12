using RPA.Core;
using RPA.Worker.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCifs.Smb;
using RPA.Tools;
namespace RPA12.Lib
{
    public class RPA12Summary
    {
        public string Factory { get; set; }
        public string PO { get; set; }
        public string MaterialNo { get; set; }
        public string Color { get; set; }
        public int ProductPlanYear { get; set; }
        public int ProductPlanMonth { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalFutureAmount { get; set; } = 0;
    }
}
