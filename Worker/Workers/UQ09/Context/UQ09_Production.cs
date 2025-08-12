using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("UQ09_Production")]
public class UQ09_Production
{
    [Key]
    public int Id { get; set; }
    public int WeekId { get; set; }
    public bool IsDone { get; set; }
    [MaxLength(16)]
    public string LastUpdatedUser { get; set; } = string.Empty;
    public DateTimeOffset LastUpdatedTime { get; set; }
}