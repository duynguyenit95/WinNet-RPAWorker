
using System;
namespace RPA.Core
{
    /// <summary>
    /// Base auditable entity for database model
    /// </summary>
    public class Entity
    {
        public int ID { get; set; }
        public string LastUpdatedBy { get; set; } = "System";
        public DateTime LastUpdatedTime { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool CanRemoveOrDisable { get; set; } = true;
    }
}
