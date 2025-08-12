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
    public class RPA12Input
    {
        public string PO { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime FileDate { get; set; }
    }
}
