using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQ09.Model
{
    public class MaterialPO
    {
        public string MO_NUM { get; set; } = string.Empty;
        public string MO_DTL_NUM { get; set; } = string.Empty;
        public string MFCTRY_CD_SHIP_FROM { get; set; } = string.Empty;
        public string MFCTRY_CD_SHIP_TO { get; set; } = string.Empty;
        public string TRNSPT_MODE { get; set; } = string.Empty;
        public string SPLR_MTRL_CD { get; set; } = string.Empty;
        public string SPLR_CL_CD { get; set; } = string.Empty;
        public string DLVR_LCTN { get; set; } = string.Empty;
        public string SHIP_DLVR_DATE { get; set; } = string.Empty;
        public string RCV_DLVR_DATE { get; set; } = string.Empty;
        public string MO_QTY { get; set; } = string.Empty;
        public string MO_ISSD_DATE { get; set; } = string.Empty;
        public string MO_STS_NAME { get; set; } = string.Empty;
    }
}
