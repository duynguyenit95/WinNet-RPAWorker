using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RPA.Core.Models.SAP.ZACCOUNT
{
    [Table("ZACCOUNT")]
    public class ZAccountResult
    {
        public int Id { get; set; }
        public string Account { get; set; } //BNAME
        public string SystemName { get; set; } //NAME_LAST
        public string EmpId { get; set; } //NICKNAME
        public string ValidFrom { get; set; } //GLTGV
        public string ValidTo { get; set; } //GLTGB
        public string Department1 { get; set; } //ROOMNUMBER
        public string Department2 { get; set; } //FLOOR
        public string Department3 { get; set; } //DEPARTMENT
        public string Department4 { get; set; } //FUNCTION
        public string Email { get; set; } //EMAIL
        public string Phone { get; set; } //TEL_NUMBER
        public string ShortPhone { get; set; } //TEL_EXTENS
        public string MobilePhone { get; set; } //MOBLIE
        public string Company { get; set; } //SORT1
        public string Field { get; set; } //SORT2
        public string ProcessManager { get; set; } //NAMEMIDDLE
        public string LastLogin { get; set; } //TRDAT
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }

}
