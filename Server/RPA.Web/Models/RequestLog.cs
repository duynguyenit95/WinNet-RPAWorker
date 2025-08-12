using System;
using System.ComponentModel.DataAnnotations;

namespace RPA.Web.Models
{
    public class RequestLog
    {
        [Key]
        public int ID { get; set; }

        [MaxLength(64)]
        public string Controller { get; set; }
        [MaxLength(128)]
        public string Action { get; set; }
        public string QueryURL { get; set; }
        public string QueryString { get; set; }
        public string FormString { get; set; }

        [MaxLength(20)]
        public string User { get; set; }
        public DateTime RequestTime { get; set; } = DateTime.Now;
    }
}
