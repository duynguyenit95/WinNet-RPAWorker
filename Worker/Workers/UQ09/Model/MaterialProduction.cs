using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQ09.Model
{
    public class MaterialProduction
    {
        public string DATA_TYPE_CD { get; set; } = string.Empty;
        public string PRD_MFCTRY_CD { get; set; } = string.Empty;
        public string PRD_PHYSCL_FCTRY_CD { get; set; } = string.Empty;
        public string SMPL_SPLR_MTRL_CD_PRD { get; set; } = string.Empty;
        public string SLS_CL_SPLR_CL_CD_PRD { get; set; } = string.Empty;
        public string PRD_LOT_ID { get; set; } = string.Empty;
        public string PRD_START_DATE { get; set; } = string.Empty;
        public string SPLR_MTRL_CD_CNSMPTN { get; set; } = string.Empty;
        public string SPLR_CL_CD_CNSMPTN { get; set; } = string.Empty;
        public string INPT_PO_NUM { get; set; } = string.Empty;
        public string CNSMPTN_MTRL_PRD_LOT_ID { get; set; } = string.Empty;
        public string PRD_WK { get; set; } = string.Empty;
        public string CNSMPTN_ACTL_QTY { get; set; } = string.Empty;
        public string PRD_PLN_QTY { get; set; } = string.Empty;
        public string PRD_ACTL_QTY { get; set; } = string.Empty;
    }
}
