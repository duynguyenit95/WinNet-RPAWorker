using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UQ09.Enum;

namespace UQ09.Model
{
    public class UQParam
    {
        public UQArea Area { get; set; }
        public string RequestPath { get; set; } = string.Empty;
        public string ResultPath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public bool UQ09 { get; set; }
        public bool UQ09Production { get; set; }
    }
}
