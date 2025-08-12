using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("UQ09_ProductionDetail")]
public class UQ09_ProductionDetail
{
    [Key]
    public int Id { get; set; }
    public int ProductionId { get; set; }
    [MaxLength(4)]
    public string PRD_MFCTRY_CD { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string SMPL_SPLR_MTRL_CD_PRD { get; set; } = string.Empty;
    [MaxLength(300)]
    public string SLS_CL_SPLR_CL_CD_PRD { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string SPLR_MTRL_CD_CNSMPTN { get; set; } = string.Empty;
    [MaxLength(100)]
    public string SPLR_CL_CD_CNSMPTN { get; set; } = string.Empty;
    public DateTime PRD_WK { get; set; }
    public decimal CNSMPTN_ACTL_QTY { get; set; }
}
